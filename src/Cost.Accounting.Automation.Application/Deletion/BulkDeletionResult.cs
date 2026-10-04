using TS.Result;

namespace Cost.Accounting.Automation.Application.Deletion;

/// <summary>
/// <see cref="BulkDeletionRunner{TEntity}"/> sonucunu kullanıcıya gösterilecek
/// metne çevirir. Böylece her toplu silme komutu aynı mesaj düzenini paylaşır.
/// </summary>
public static class BulkDeletionResult
{
    /// <summary>
    /// Başarılı sonuçta ilişkili kullanım yoksa sadece silme mesajı, varsa
    /// ilişkili kullanım notu eklenmiş mesaj üretir.
    /// </summary>
    public static Result<string> ToMessage(Result<BulkDeletionOutcome> result, string entityLabel)
    {
        if (!result.IsSuccessful)
        {
            string error = result.ErrorMessages is null
                ? "İşlem başarısız oldu."
                : string.Join(Environment.NewLine, result.ErrorMessages);

            return Result<string>.Failure(error);
        }

        BulkDeletionOutcome? outcome = result.Data;
        if (outcome is null)
        {
            return Result<string>.Failure("İşlem başarısız oldu.");
        }

        if (!outcome.HasRelated)
        {
            return DeletionMessages.Deleted(outcome.DeletedCount, entityLabel);
        }

        return DeletionMessages.RelatedWarning(
            entityLabel,
            outcome.DeletedCount,
            outcome.RelatedCount);
    }
}
