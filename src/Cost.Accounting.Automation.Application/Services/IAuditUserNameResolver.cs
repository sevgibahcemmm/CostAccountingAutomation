using Cost.Accounting.Automation.Domain.Abstractions;

namespace Cost.Accounting.Automation.Application.Services;

/// <summary>
/// Denetim sütunlarındaki kullanıcı adlarını (oluşturan / güncelleyen) çözer.
///
/// <para>
/// <b>Neden ayrı bir servis gerekiyor:</b> iş kayıtları seçili yılın
/// veritabanında, kullanıcı kayıtları ise <b>master</b> veritabanında durur.
/// İki ayrı katalog olduğu için kullanıcı adı, iş kaydının SQL sorgusuna
/// <c>JOIN</c> ile eklenemez. Bu yüzden liste sorgusu önce kayıtları ve
/// denetim kimliklerini SQL'den alır, adlar ikinci bir (küçük) sorguda
/// çözülüp DTO'ya yazılır.
/// </para>
///
/// <para>
/// Bu sayede liste sorgusunun tamamı veritabanında filtrelenip
/// projekte edilir; belleğe yalnızca sonuç satırları gelir. Alternatif olan
/// "tüm tabloyu belleğe al, sonra filtrele" yaklaşımı kayıt sayısı arttıkça
 /// doğrusal olmayan biçimde yavaşlar.
/// </para>
/// </summary>
public interface IAuditUserNameResolver
{
    /// <summary>
    /// <paramref name="items"/> içindeki denetim kimliklerini karşılayan kullanıcı
    /// adlarını doldurur. Liste boşsa veritabanına hiç gitmez.
    /// </summary>
    Task FillAsync(IEnumerable<EntityDto> items, CancellationToken cancellationToken = default);
}
