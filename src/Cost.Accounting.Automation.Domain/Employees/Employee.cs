using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Employees.ValueObjects;
using Cost.Accounting.Automation.Domain.Shared;

namespace Cost.Accounting.Automation.Domain.Employees;

/// <summary>
/// Kurum çalışanı. Giriş ekranındaki <see cref="Users.User"/> kaydından
/// bağımsızdır: bu kayıt yalnızca belgelerin imza bölümünü doldurmak için
/// tutulur ve mali yıl veritabanında saklanır.
///
/// Personelin birden fazla yetkili görevi olabilir; görevler ayrı
/// <see cref="EmployeeDuty"/> kayıtlarında tutulur. Böylece aynı kişi örneğin
/// bir atölyenin şefi iken kurum genelinde muhasebe yetkilisi de olabilir.
/// </summary>
public sealed class Employee : Entity
{
    private readonly List<EmployeeDuty> _duties = [];

    private Employee()
    {
    }

    public Employee(
        FirstName firstName,
        LastName lastName,
        TRIdentityNumber identityNumber,
        EmployeeTitle title,
        string phoneNumber1,
        string phoneNumber2,
        string email,
        string? photoPath,
        bool isActive)
    {
        SetFirstName(firstName);
        SetLastName(lastName);
        SetIdentityNumber(identityNumber);
        SetTitle(title);
        SetPhoneNumber1(phoneNumber1);
        SetPhoneNumber2(phoneNumber2);
        SetEmail(email);
        SetPhotoPath(photoPath);
        SetStatus(isActive);
        ResolveDuplicateKey();
    }

    public FirstName FirstName { get; private set; } = default!;

    public LastName LastName { get; private set; } = default!;

    /// <summary>T.C. Kimlik Numarası. Bir personelin aynı TC ile iki kez kaydedilmesini engeller.</summary>
    public TRIdentityNumber IdentityNumber { get; private set; } = default!;

    /// <summary>Rütbe / görev ünvanı. Rapor imza bloklarındaki "Ünvanı" satırına basılır.</summary>
    public EmployeeTitle Title { get; private set; } = default!;

    public string PhoneNumber1 { get; private set; } = string.Empty;

    public string PhoneNumber2 { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Kurumun verdiği personel sicil numarası.
    ///
    /// <para>
    /// T.C. Kimlik Numarası'ndan ayrı ve onun yerine geçmez: sicil numarası
    /// kurum içi kayıt numarasıdır, bordro ve imza bloklarında referans olarak
    /// kullanılır. Zorunlu değildir; alanı doldurulmamış eski personel
    /// kayıtları geçerli kalır.
    /// </para>
    /// <para>
    /// Dolu girildiğinde benzersizdir: aynı sicil numarası iki personele
    /// verilemez.
    /// </para>
    /// </summary>
    public string? RegistryNumber { get; private set; }

    /// <summary>
    /// Personel fotoğrafının <see cref="Application.Services.IFileStorageService"/>
    /// köküne göreli yolu. Raporlarda fotoğraf basılmaz; kayıt ekranında önizleme
    /// ve listede avatar olarak kullanılır.
    /// </summary>
    public string? PhotoPath { get; private set; }

    public IReadOnlyCollection<EmployeeDuty> Duties => _duties;

    /// <summary>
    /// Rapor imza bloklarında "Adı Soyadı" satırına basılan tam ad.
    ///
    /// <para>
    /// Ad ve soyad <see cref="FirstName"/>/<see cref="LastName"/> değer
    /// nesneleridir ve veritabanından okunurken owned navigasyon olarak
    /// materyalize edilir. Kısmen yüklenmiş bir kayıtta (ör. yalnızca tek bir
    /// alanla çağrılan bir kod yolu) bu nesneler null olabilir; imza satırını
    /// basmaya çalışan bir ekranın bu yüzden çökmesi kabul edilemez. Bu
    /// yüzden erişim null güvenlidir.
    /// </para>
    /// </summary>
    public string FullName => $"{FirstName?.Value} {LastName?.Value}".Trim();

    /// <summary>
    /// Aynı TC ve aynı ünvan ikilisini iki kez kaydetmeyi engeller. Ünvan
    /// bilgisi ikinci bir ayırt edici alan olduğu için şemaya alınır: aynı kişi
    /// farklı ünvanla ikinci kez eklenirse kayıt yine de yakalanır.
    /// </summary>
    public static string? BuildDuplicateKey(string identityNumber)
        => DuplicateKeyRule.From(identityNumber);

    public void ResolveDuplicateKey()
        => SetDuplicateKey(BuildDuplicateKey(IdentityNumber.Value));

    public void SetFirstName(FirstName firstName) => FirstName = firstName;

    public void SetLastName(LastName lastName) => LastName = lastName;

    public void SetIdentityNumber(TRIdentityNumber identityNumber)
    {
        IdentityNumber = identityNumber;
        ResolveDuplicateKey();
    }

    public void SetTitle(EmployeeTitle title) => Title = title;

    public void SetPhoneNumber1(string phoneNumber1) => PhoneNumber1 = phoneNumber1;

    public void SetPhoneNumber2(string phoneNumber2) => PhoneNumber2 = phoneNumber2;

    public void SetEmail(string email) => Email = email;

    /// <param name="registryNumber">Sicil numarası; boş bırakılırsa <c>null</c> olur.</param>
    public void SetRegistryNumber(string? registryNumber)
        => RegistryNumber = string.IsNullOrWhiteSpace(registryNumber)
            ? null
            : registryNumber.Trim();

    public void SetPhotoPath(string? photoPath) => PhotoPath = photoPath;

    /// <summary>
    /// Görev listesini verilenle tamamen değiştirir. Kaydetme sırasında eski
    /// kayıtlar silinip yenileri yazılır; görev geçmişi tutulmaz, çünkü
    /// raporlar yalnızca güncel yetkiliyi göstermektedir.
    /// </summary>
    public void ReplaceDuties(IEnumerable<EmployeeDuty> duties)
    {
        _duties.Clear();
        _duties.AddRange(duties);
    }
}