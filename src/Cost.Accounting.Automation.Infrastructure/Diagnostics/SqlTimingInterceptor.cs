using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Cost.Accounting.Automation.Infrastructure.Diagnostics;

/// <summary>
/// Her SQL komutunun gerçek yürütme süresini (EF translation dahil değil; SqlClient round-trip)
/// Özellik katsayımıyla VS Debug çıktısına yazar. Amaç: ~25 sn'lik katalog gecikmesinin
/// EF tarafında mı (query compilation/translation) yoksa ADO.NET/SQL tarafında mı olduğunu ayırmak.
/// </summary>
internal sealed class SqlTimingInterceptor : DbCommandInterceptor
{
    private readonly System.Runtime.CompilerServices.ConditionalWeakTable<DbCommand, Track> _tracks = new();

    private sealed class Track
    {
        public bool Started;
        public long CreatedMs;
        public Stopwatch Exec = new();
        public string? Sql;
    }

    public override DbCommand CommandCreated(CommandEndEventData eventData, DbCommand result)
    {
        var track = _tracks.GetOrCreateValue(result);
        track.CreatedMs = Stopwatch.GetTimestamp();
        track.Sql = result.CommandText;
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

    private void MarkExecuting(DbCommand command)
    {
        if (!_tracks.TryGetValue(command, out Track? track) || track.Started)
        {
            return;
        }

        track.Started = true;
        track.Exec.Restart();
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
        if (_tracks.TryGetValue(command, out Track? track))
        {
            long totalMs = (long)Stopwatch.GetElapsedTime(track.CreatedMs).TotalMilliseconds;
            long execMs = track.Exec.ElapsedMilliseconds;
            string sql = Shorten(string.IsNullOrWhiteSpace(track.Sql) ? command.CommandText : track.Sql);
            Debug.WriteLine($"[SQL] exec={execMs}ms total={totalMs}ms sql={sql}");
            _tracks.Remove(command);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Shorten(string sql)
    {
        string oneLine = sql.Replace("\r", " ").Replace("\n", " ");
        return oneLine.Length <= 180 ? oneLine : oneLine[..180] + "...";
    }
}