using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Cost.Accounting.Automation.Infrastructure.Diagnostics;

/// <summary>
/// Her SQL komutunun gerçek yürütme süresini (EF translation dahil değil; SqlClient round-trip)
/// Özellik katsayımıyla VS Debug çıktısına yazar. Amaç: ~25 sn'lik katalog gecikmesinin
/// EF tarafında mı (query compilation/translation) yoksa ADO.NET/SQL tarafında mı olduğunu ayırmak.
/// </summary>
internal sealed class SqlTimingInterceptor : DbCommandInterceptor
{
    /// <summary>
    /// Loglanan SQL uzunluğu. Kısaltma kasten yapılıyordu ama <c>WHERE</c> ve
    /// <c>JOIN</c> kısımları kesildiği için hangi komutun yavaş olduğu
    /// belirlenemiyordu; bu yüzden sınır yükseltildi.
    /// </summary>
    private const int MaxLoggedSqlLength = 2000;

    /// <summary>
    /// Bu süreyi aşan komutların <b>tam</b> SQL'i ve parametreleri
    /// <c>logs\slow-queries.log</c> dosyasına yazılır. Konsol çıktısı
    /// kısaltıldığı için yavaş sorgunun gerçek şekli (özellikle <c>WHERE</c> ve
    /// tüm <c>JOIN</c>'ler) görülemiyordu; dosya bunu olduğu gibi kaydeder ve
    /// sorgu veritabanında doğrudan çalıştırılabilir.
    /// </summary>
    private const int SlowCommandThresholdMs = 1000;

    /// <summary>
    /// Uzun komutların yaşam döngüsünü <c>logs\sql-trace.log</c> dosyasına
    /// mutlak zaman damgasıyla yazar. <c>Stopwatch</c> süresi tek başına
    /// belirsiz olduğu için (aynı komut birden fazla kez çalıştırılabilir)
    /// duvar saati kaydı, sürenin <c>created</c> → <c>begin</c> → <c>end</c>
    /// arasında hangi aralıkta geçtiğini kesin gösterir.
    /// </summary>
    private const int TracedSqlMinLength = 2000;

    private static int _traceSequence;

    private static readonly object TraceLock = new();

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
            string sql = Shorten(string.IsNullOrWhiteSpace(track.Sql) ? command.CommandText : track.Sql);
            Debug.WriteLine($"[SQL] exec={execMs}ms total={totalMs}ms sql={sql}");
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
    /// dosyasına yazar. Yalnızca uzun komutlar izlenir; dosya yazımı tanı
    /// amaçlıdır ve hata durumunda yüklemeyi bozmamalıdır.
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

            lock (TraceLock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(TracePath)!);
                File.AppendAllText(
                    TracePath,
                    $"{DateTimeOffset.Now:O} [{stage}] id={track.Id} len={sqlLength} thread={Environment.CurrentManagedThreadId}{suffix}\n");
            }
        }
        catch
        {
        }
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