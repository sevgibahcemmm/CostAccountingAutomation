using Cost.Accounting.Automation.Domain.Employees;

namespace Cost.Accounting.Automation.Application.Employees;

/// <summary>
/// Personel kaydıyla ilgili kullanıcıya gösterilen ortak mesajlar. Komut
/// işleyicileri ve düzenleme formu aynı metinleri kullanır; böylece "TC
/// zaten kayıtlı" uyarısı iki yerde farklı yazılmaz.
/// </summary>
public static class EmployeeMessages
{
    public const string NotFound = "Personel bulunamadı";

    public const string DuplicateIdentityNumber =
        "Bu TC kimlik numarası ile kayıtlı bir personel zaten mevcut";

    public const string DuplicateRegistryNumber =
        "Bu sicil numarası ile kayıtlı bir personel zaten mevcut";

    public const string RegistryNumberTooLong =
        "Sicil numarası en fazla 50 karakter olabilir";

    public const string DuplicateDuty =
        "Aynı görev birden fazla kez eklenmiş. Her görevi yalnızca bir kez tanımlayın.";

    public const string InvalidWorkshop =
        "Seçilen atölye bulunamadı. Görevlendirme için geçerli bir atölye seçin.";

    public const string NoDuty =
        "En az bir yetkili görev tanımlayın. Görevi olmayan personel raporlarda imza satırına basılamaz.";

    public const string UnselectedRole =
        "Bir görev satırında yetkili görev seçilmemiş. Tablodaki her satır için görev seçin.";

    public const string UnknownRole =
        "Seçilen yetkili görev bulunamadı. Görev tanımı silinmiş olabilir; görev listesini yenileyin.";
}

/// <summary>
/// Sicil numarasının kayda yazılmadan önce aldığı biçim. Sicil numarası
/// isteğe bağlı olduğu için boş girdiler <c>null</c>'a indirgenir; veritabanında
/// boş dizeyle <c>NULL</c> karışmasın ve benzersizlik denetimi yalnızca gerçek
/// numaraları görsün diye bu tek giriş noktasından geçirilir.
/// </summary>
public static class EmployeeRegistryNumber
{
    public static string? Normalize(string? registryNumber)
        => string.IsNullOrWhiteSpace(registryNumber) ? null : registryNumber.Trim();
}