using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Cost.Accounting.Automation.Infrastructure.Diagnostics;

/// <summary>
/// Her SQL komutunun gerçek yürütme süresini ölçer (EF translation dahil değil;
/// SqlClient round-trip) ve önemli bulduklarını tanı amaçlı kaydeder:
/// <c>SqlLogThresholdMs</c> üstü/uzun komutlar konsola, uzun komutların yaşam
/// döngüsü <c>logs\sql-trace.log</c> dosyasına, <c>SlowCommandThresholdMs</c>
/// üstü komutların tam SQL'i <c>logs\slow-queries.log</c> dosyasına yazılır.
/// Konsol ve dosya yazımının arayüz thread'ini bekletmemesi için hem konsol
/// logu eşikli hem de dosya yazımı arka plan kuyrukludur; bkz. <see cref="Trace"/>.
/// </summary>
internal sealed class SqlTimingInterceptor : DbCommandInterceptor
{
    /// <summary>
    /// Konsola (VS Çıktı penceresi) yazılan SQL uzunluğu.
    ///
    /// Kısıtlamanın sebebi: <c>Debug.WriteLine</c>, debugger ekliyken
    /// <c>OutputDebugString</c> üzerinden Visual Studio'ya gider ve debugger
    /// satırı okuyana kadar çağıran thread'i (arayüz thread'ini) bloklar.
    /// Uzun sorgu metinleri Çıktı penceresini doldurduğunda bu bloklama
    /// gözle görülür donmalara dönüşüyordu. Tam metin gerektiğinde
    /// <c>SQL_TRACE_VERBOSE=1</c> ortam değişkeniyle eski davranış geri
    /// açılabilir (bkz. <see cref="VerboseSqlLog"/>).
    /// </summary>
    private const int MaxLoggedSqlLength = 2000;

    /// <summary>
    /// Konsol logunun eşiği. Süresi bu değeri aşmayan komutlar
    /// <c>Debug.WriteLine</c> ile yazılmaz; amaç yalnızca dikkat çekmeye
    /// değer sorguları gösterip Çıktı penceresini (ve dolayısıyla debugger'ı)
    /// gereksiz satırlarla kilitlememektir. Tümünü görmek için
    /// <c>SQL_TRACE_VERBOSE=1</c>.
    /// </summary>
    private const long SqlLogThresholdMs = 50;

    /// <summary>
    /// <c>SQL_TRACE_VERBOSE=1</c> ortam değişkeni verildiğinde süresi
    /// <see cref="SqlLogThresholdMs"/> altındaki komutlar da konsola yazılır
    /// (eski, her sorguyu loglayan davranış).
    /// </summary>
    private static readonly bool VerboseSqlLog =
        Environment.GetEnvironmentVariable("SQL_TRACE_VERBOSE") == "1";

    /// <summary>
    /// Yavaş komutun tam SQL'i ve parametreleri <c>logs\slow-queries.log</c>
    /// dosyasına yazılır. Bu eşik, komutun gerçek yürütme süresidir
    /// (EF translation dahil değil; SqlClient round-trip).
    /// </summary>
    private const int SlowCommandThresholdMs = 1000;

    /// <summary>
    /// Uzun komutların yaşam döngüsünü <c>logs\sql-trace.log</c> dosyasına
    /// mutlak zaman damgasıyla yazar. <c>Stopwatch</c> süresi tek başına
    /// belirsiz olduğu için (aynı komut birden fazla kez çalıştırılabilir)
    /// duvar saati kaydı, sürenin <c>created</c> → <c>begin</c> → <c>end</c>
    /// arasında hangi aralıkta geçtiğini kesin gösterir.
    ///
    /// Satırlar önce kuyruğa alınır, dosya yazımı arka planda tek bir
    /// thread'de yapılır; bkz. <see cref="EnsureTraceWriter"/>.
    /// </summary>
    private const int TracedSqlMinLength = 2000;

    private static int _traceSequence;

    /// <summary>
    /// <c>sql-trace.log</c> satırları için arka plan kuyruğu.
    ///
    /// Eskiden yazım <see cref="TraceLock"/> adlı global kilit altında
    /// senkron yapılıyordu; arka plandaki bir sorgu kilidi tutarken arayüz
    /// thread'i disk gecikmesinde bekliyordu (gözle görülür donma). Kuyruk
    /// sayesinde arayüz thread'i yalnızca kuyruğa ekler ve hemen döner.
    /// Süreç aniden sonlanırsa kuyrukta kalan satırlar yazılmaz; bu, tanı
    /// kaydının uygulama akışından daha önemli olmadığı için kabul edilir.
    /// </summary>
    private static readonly BlockingCollection<string> TraceQueue = new();

    private static int _traceWriterStarted;

    private static string TracePath =>
        Path.Combine(AppContext.BaseDirectory, "logs", "sql-trace.log");

    private readonly System.Runtime.CompilerServices.ConditionalWeakTable<DbCommand, Track> _tracks = new();

    private sealed class Track
    {
        public int Id;
        public bool Started;
        public long CreatedMs;
        public Stopwatch Exec = new();
        public string? Sql;
    }

    public override DbCommand CommandCreated(CommandEndEventData eventData, DbCommand result)
    {
        var track = _tracks.GetOrCreateValue(result);
        track.Id = Interlocked.Increment(ref _traceSequence);
        track.CreatedMs = Stopwatch.GetTimestamp();
        track.Sql = result.CommandText;
        track.Started = false;
        track.Exec.Reset();
        Trace(track, "created", result.CommandText.Length);
        return result;
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        MarkExecuting(command);
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        MarkExecuting(command);
        return ValueTask.FromResult(result);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
    {
        MarkExecuting(command);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        MarkExecuting(command);
        return ValueTask.FromResult(result);
    }

    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        MarkExecuting(command);
        return result;
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
    {
        MarkExecuting(command);
        return ValueTask.FromResult(result);
    }

    /// <summary>
    /// Yürütme sayacını başlatır.
    ///
    /// Buradaki <c>Restart()</c> bilinçli olarak koşulsuz çağrılır. EF Core
    /// normalde bir komut için <c>*Executing</c> ve <c>*Executed</c> çağrılarını
    /// birebir eşleştirir ve <see cref="LogExec"/> track'i attığı için sayaç
    /// zaten doğru sıfırlanır. Yine de <c>*Executed</c>'in çağrılmadığı bir
    /// yolda (komut iptali, beklenmedik hata) track <c>Started = true</c> halinde
    /// kalabilirdi; koşulsuz yeniden başlatma bu durumda yanlış süre
    /// raporlanmasını engeller.
    ///
    /// <b>Not:</b> Daha önce buradaki koruma kaldırılmadan önce
    /// "exec=25052ms total=25052ms" satırları görüldü ve bu bir ölçüm hatası
    /// sanıldı. Yapılan ölçümler bunun <b>yanlış</b> olduğunu gösterdi: aynı
    /// sorgu veritabanında 1 ms sürüyordu. <c>crash.log</c>'daki
    /// <c>DB(Query) 25.828 ms</c> satırı da bir hata değildi — <c>N0</c>
    /// biçimi Türkçe kültürde binlik ayracı olarak <c>.</c> kullandığı için
    /// 25828 ms "25.828" olarak yazılmış. Gerçek sorun istemci tarafındaydı.
    /// </summary>
    private void MarkExecuting(DbCommand command)
    {
        if (!_tracks.TryGetValue(command, out Track? track))
        {
            return;
        }

        track.Started = true;
        track.Exec.Restart();
        Trace(track, "begin", command.CommandText.Length);
    }

    public override DbDataReader ReaderExecuted(
        DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        LogExec(command, eventData);
        return result;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
    {
        LogExec(command, eventData);
        return ValueTask.FromResult(result);
    }

    public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
    {
        LogExec(command, eventData);
        return result;
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        LogExec(command, eventData);
        return ValueTask.FromResult(result);
    }

    public override object? ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object? result)
    {
        LogExec(command, eventData);
        return result;
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, object? result, CancellationToken cancellationToken = default)
    {
        LogExec(command, eventData);
        return ValueTask.FromResult(result);
    }

    public override InterceptionResult DataReaderDisposing(
        DbCommand command, DataReaderDisposingEventData eventData, InterceptionResult result)
    {
        return result;
    }

    private void LogExec(DbCommand command, CommandExecutedEventData eventData)
    {
        if (!_tracks.TryGetValue(command, out Track? track) || !track.Started)
        {
            return;
        }

        try
        {
            long totalMs = (long)Stopwatch.GetElapsedTime(track.CreatedMs).TotalMilliseconds;
            long execMs = track.Exec.ElapsedMilliseconds;

            // Konsol logu yalnızca dikkat çekmeye değer komutlar için yazılır;
            // her sorgunun OutputDebugString ile debugger'ı bekletmesi arayüzde
            // donmaya yol açıyordu. Tümünü görmek için SQL_TRACE_VERBOSE=1;
            // uzun komutların yaşam döngüsü ayrıca logs\sql-trace.log'a yazılır.
            if (VerboseSqlLog || execMs >= SqlLogThresholdMs || totalMs >= SqlLogThresholdMs)
            {
                string sql = Shorten(string.IsNullOrWhiteSpace(track.Sql) ? command.CommandText : track.Sql);
                Debug.WriteLine($"[SQL] exec={execMs}ms total={totalMs}ms sql={sql}");
            }

            Trace(track, "end", command.CommandText.Length, execMs, totalMs);

            if (execMs >= SlowCommandThresholdMs)
            {
                DumpSlowCommand(command, execMs, totalMs);
            }
        }
        finally
        {
            // Sayaç kullanıldıktan sonra track atılmalı; aksi hâlde aynı komut
            // yeniden kullanıldığında eski yürütme bilgisiyle karışır.
            _tracks.Remove(command);
        }
    }

    /// <summary>
    /// Yavaş komutun tam SQL'ini ve parametrelerini <c>logs\slow-queries.log</c>
    /// dosyasına yazar. Dosya, sorgunun veritabanında elle çalıştırılıp
    /// <c>SET STATISTICS IO/TIME ON</c> ile incelenebilmesi için tam metni
    /// içermelidir.
    /// </summary>
    private static void DumpSlowCommand(DbCommand command, long execMs, long totalMs)
    {
        try
        {
            var builder = new StringBuilder();
            builder.AppendLine($"===== {DateTimeOffset.Now:O} | exec={execMs}ms total={totalMs}ms =====");

            foreach (DbParameter parameter in command.Parameters)
            {
                string value = parameter.Value switch
                {
                    null => "NULL",
                    byte[] bytes => $"0x{Convert.ToHexString(bytes)}",
                    _ => Convert.ToString(parameter.Value, CultureInfo.InvariantCulture) ?? "?"
                };

                builder.AppendLine($"-- {parameter.ParameterName} = {value} [{parameter.DbType}]");
            }

            builder.AppendLine(command.CommandText);
            builder.AppendLine();

            string directory = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(directory);
            File.AppendAllText(Path.Combine(directory, "slow-queries.log"), builder.ToString());
        }
        catch
        {
            // Tanı kaydı üretmek yükleme akışını bozmamalı.
        }
    }

    /// <summary>
    /// Komut yaşam döngüsünün bir aşamasını duvar saatiyle <c>logs\sql-trace.log</c>
    /// dosyasına yazar. Yalnızca uzun komutlar izlenir.
    ///
    /// Satır kuyruğa alınır, dosya yazımı arka planda yapılır; böylece
    /// arayüz thread'i disk gecikmesinde beklemez. Dosya yazımı tanı amaçlıdır
    /// ve hata durumunda yüklemeyi bozmamalıdır.
    /// </summary>
    private static void Trace(Track track, string stage, int sqlLength, long? execMs = null, long? totalMs = null)
    {
        if (sqlLength < TracedSqlMinLength)
        {
            return;
        }

        try
        {
            string suffix = execMs.HasValue
                ? $" exec={execMs}ms total={totalMs}ms"
                : string.Empty;

            // thread değeri kuyruklarken (üretici thread'de) alınır; yazımın
            // arka plana taşınması kaydın hangi thread'de üretildiğini değiştirmez.
            TraceQueue.TryAdd(
                $"{DateTimeOffset.Now:O} [{stage}] id={track.Id} len={sqlLength} thread={Environment.CurrentManagedThreadId}{suffix}\n");

            EnsureTraceWriter();
        }
        catch
        {
        }
    }

    /// <summary>
    /// Arka plan yazıcısını ilk çağrıda bir kez başlatır.
    ///
    /// Eski sürümde dosyaya global bir kilit altında senkron yazılıyordu. Bir
    /// thread kilit tutup disk yazarken diğer thread'ler (özellikle arayüz
    /// thread'i) kilidi bekleyerek takılıyordu. Artık satırlar kuyruğa
    /// alınır; diske dökme işlemi tek bir arka plan thread'inde, kilitsiz
    /// yapılır.
    /// </summary>
    private static void EnsureTraceWriter()
    {
        if (Interlocked.Exchange(ref _traceWriterStarted, 1) != 0)
        {
            return;
        }

        var writer = new Thread(() =>
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(TracePath)!);

                foreach (string line in TraceQueue.GetConsumingEnumerable())
                {
                    File.AppendAllText(TracePath, line);
                }
            }
            catch
            {
                // Tanı kaydı uygulama akışını bozmamalı.
            }
        })
        {
            IsBackground = true,
            Name = "SqlTraceWriter",
        };

        writer.Start();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Shorten(string sql)
    {
        string oneLine = sql.Replace("\r", " ").Replace("\n", " ");
        return oneLine.Length <= MaxLoggedSqlLength
            ? oneLine
            : oneLine[..MaxLoggedSqlLength] + $"... (+{oneLine.Length - MaxLoggedSqlLength} karakter)";
    }
}