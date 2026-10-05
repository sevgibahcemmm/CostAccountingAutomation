namespace Cost.Accounting.Automation.Application.Services;


public interface IClaimContext
{
Guid GetUserId();
    Guid GetCompanyId();
    string GetRoleName();

    /// <summary>
    /// Oturumu açan kullanıcının adı soyadı.
    ///
    /// <para>
    /// Kullanıcının kendi adını gösteren ekranlar (örn. mesaj geçmişinde
    /// "gönderen" başlığı) bunu kullanır; böylece her mesaj satırı için
    /// kullanıcı tablosundan ayrı sorgu atmaya gerek kalmaz.
    /// </para>
    /// </summary>
    string? GetUserFullNameOrDefault();

    Guid? GetUserIdOrDefault();
}