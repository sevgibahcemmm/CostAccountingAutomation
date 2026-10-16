using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.Deletion;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.DirectProcurement;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.Application.DirectProcurement;

/// <summary>
/// Tekil silme de toplu silmeyle aynı koruma yolunu kullanır; böylece
/// "alt kaydı olan dosya silinemez" kuralı tek yerde tanımlı kalır.
/// </summary>
[Permission("direct_procurement:delete")]
public sealed record DirectProcurementFileDeleteCommand(
    Guid Id) : IRequest<Result<string>>;

internal sealed class DirectProcurementFileDeleteCommandHandler(
    IDirectProcurementFileRepository fileRepository) : IRequestHandler<DirectProcurementFileDeleteCommand, Result<string>>
{
    public async Task<Result<string>> Handle(DirectProcurementFileDeleteCommand request, CancellationToken cancellationToken)
    {
        var runner = new BulkDeletionRunner<DirectProcurementFile>(
            fileRepository,
            (ids, token) => fileRepository.GetDeletionCheckAsync(ids, token));

        Result<BulkDeletionOutcome> result = await runner.RunAsync(
            [request.Id],
            "doğrudan temin dosyası",
            cancellationToken);

        return BulkDeletionResult.ToMessage(result, "doğrudan temin dosyası");
    }
}

[Permission("direct_procurement:delete")]
public sealed record BulkDeleteDirectProcurementFilesCommand(
    List<Guid> Ids) : IRequest<Result<string>>;

internal sealed class BulkDeleteDirectProcurementFilesCommandHandler(
    IDirectProcurementFileRepository fileRepository) : IRequestHandler<BulkDeleteDirectProcurementFilesCommand, Result<string>>
{
    public async Task<Result<string>> Handle(BulkDeleteDirectProcurementFilesCommand request, CancellationToken cancellationToken)
    {
        var runner = new BulkDeletionRunner<DirectProcurementFile>(
            fileRepository,
            (ids, token) => fileRepository.GetDeletionCheckAsync(ids, token));

        Result<BulkDeletionOutcome> result = await runner.RunAsync(
            request.Ids,
            "doğrudan temin dosyası",
            cancellationToken);

        return BulkDeletionResult.ToMessage(result, "doğrudan temin dosyası");
    }
}

[Permission("direct_procurement:delete")]
public sealed record DirectProcurementFileRestoreCommand(
    Guid Id) : IRequest<Result<string>>;

internal sealed class DirectProcurementFileRestoreCommandHandler(
    IDirectProcurementFileRepository fileRepository) : IRequestHandler<DirectProcurementFileRestoreCommand, Result<string>>
{
    public async Task<Result<string>> Handle(DirectProcurementFileRestoreCommand request, CancellationToken cancellationToken)
    {
        IdentityId fileId = new(request.Id);

        DirectProcurementFile? file = await fileRepository.GetByIdIncludingDeletedAsync(fileId, cancellationToken);

        if (file is null)
        {
            return Result<string>.Failure("Doğrudan temin dosyası bulunamadı");
        }

        fileRepository.Restore(file);

        return "Doğrudan temin dosyası geri yüklendi";
    }
}