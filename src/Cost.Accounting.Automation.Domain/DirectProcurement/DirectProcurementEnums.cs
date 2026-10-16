using System.ComponentModel.DataAnnotations;

namespace Cost.Accounting.Automation.Domain.DirectProcurement;

/// <summary>
/// Doğrudan temin dosyasının iş türü. Kaynak sistemdeki <c>IHALE.IHALETURU</c>
/// alanıdır ve 22/d parasal limitlerinin dört kategorisiyle birebir örtüşür.
/// </summary>
public enum ProcurementCategoryType
{
    [Display(Name = "Yapım")]
    Construction = 1,

    [Display(Name = "Hizmet")]
    Service = 2,

    [Display(Name = "Mal Alımı")]
    Goods = 3,

    [Display(Name = "Danışmanlık")]
    Consultancy = 4
}

/// <summary>
/// Doğrudan temin dosyasının ihale tipi. Kaynak sistemdeki
/// <c>IHALE.IHALETIPI</c> alanıdır.
/// </summary>
/// <remarks>
/// Kaynak veritabanında yalnızca <see cref="DirectProcurement"/> değeri
/// görülmüştür; diğer üyeler KİK ihale tipi sınıflamasından gelir ve yeni
/// kayıtlarda kullanılabilir.
/// </remarks>
public enum ProcurementFileType
{
    [Display(Name = "Mal ve Hizmet Alımı")]
    GoodsAndService = 1,

    [Display(Name = "Yapım İşi")]
    ConstructionWork = 2,

    [Display(Name = "Danışmanlık Hizmeti")]
    ConsultancyService = 3,

    [Display(Name = "Doğrudan Temin")]
    DirectProcurement = 4
}

/// <summary>
/// Temin şekli. Kaynak sistemdeki <c>IHALE.TEMINSEKLI</c> alanıdır ve
/// 4734 sayılı Kanun'un 22. maddesinin bentlerine karşılık gelir. Değerler
/// <c>TEMINTIPI</c> tanım tablosundaki <c>ID</c> sütunuyla birebir aynıdır;
/// tanım tablosundaki <c>SILINMEZ</c> bayrağı burada bilinçli olarak modellenmez,
/// çünkü geçmişe dönük kayıtların silinememesi gereği bir iş kuralıdır, veri
/// tipi değil.
/// </remarks>
public enum ProcurementFlowType
{
    [Display(Name = "22/a - Tek kişi")]
    Article22A = 1,

    [Display(Name = "22/b - Özel hak sahibi")]
    Article22B = 2,

    [Display(Name = "22/c - Standardizasyon")]
    Article22C = 3,

    [Display(Name = "22/d* - Parasal limit içi")]
    Article22D1 = 4,

    [Display(Name = "22/d** - Temsilciler")]
    Article22D2 = 5,

    [Display(Name = "22/e - Taşınmaz alım/kiralama")]
    Article22E = 6,

    [Display(Name = "22/f - İlaç ve sağlık ürünleri")]
    Article22F = 7,

    [Display(Name = "22/g - Uluslararası tahkim davaları")]
    Article22G = 8,

    [Display(Name = "22/h - Fikri haklar")]
    Article22H = 9,

    [Display(Name = "22/ğ - TÜRKİYE/İŞKUR görevleri")]
    Article22G2 = 10,

    [Display(Name = "22/i - Seçim yenileme")]
    Article22I = 11,

    [Display(Name = "3/e - Adalet/SHÇKK kurumları")]
    Article3E = 12,

    [Display(Name = "Diğer - 5018 sayılı Kanun 33. madde")]
    Other = 13
}

/// <summary>
/// Yaklaşık maliyet hesaplama şekli (kaynak: <c>IHALE.YMHESAPSEKLI</c>).
/// </summary>
/// <remarks>
/// Kaynak ekranda <c>TcxDBRadioGroup</c> olarak yalnızca <b>iki</b> seçenek
/// sunulur; bu yüzden enum da tam olarak iki değer içerir. Daha önceki taslakta
/// bulunan dört değer, kaynakta karşılığı bulunmayan bir alanı işaret ediyordu.
/// </remarks>
public enum CostEstimationMethodType
{
    [Display(Name = "Ortak Birim Fiyat")]
    CommonUnitPrice = 0,

    [Display(Name = "En Düşük Birim Fiyat")]
    LowestUnitPrice = 1
}

/// <summary>
/// Bütçe fişinin türü. Kaynak sistemdeki <c>IHBUTCE.BUTCETIP</c> alanıdır ve
/// <c>BUTCEHHTIP</c> tanım tablosunun <c>TIPID</c> değerleriyle birebir aynıdır.
/// </summary>
public enum BudgetVoucherType
{
    [Display(Name = "Açılış Fişi")]
    Opening = 10,

    [Display(Name = "Serbest Giriş")]
    FreeEntry = 20,

    [Display(Name = "Ek Ödenek")]
    AdditionalAppropriation = 30,

    [Display(Name = "Transfer")]
    Transfer = 40,

    [Display(Name = "Blokaj")]
    Block = 50,

    [Display(Name = "Nakit Fişi")]
    CashVoucher = 60
}

/// <summary>
/// Dosyanın süreç içindeki konumu. Kaynak sistem bunu ayrı bir alan olarak
/// tutmaz; dosyanın doldurulmuş bölümlerinden türetilir. Burada açık bir
/// durum olarak tutulması, liste ekranında tek sorguda süzme ve renklendirme
/// yapılabilmesi içindir.
/// </summary>
public enum DirectProcurementStage
{
    [Display(Name = "Taslak")]
    Draft = 0,

    [Display(Name = "İhtiyaç Listesi")]
    RequirementList = 10,

    [Display(Name = "Teklif Alındı")]
    OffersReceived = 20,

    [Display(Name = "Yaklaşık Maliyet")]
    CostEstimated = 30,

    [Display(Name = "Onay Belgesi")]
    Approved = 40,

    [Display(Name = "Sözleşme")]
    Contract = 50,

    [Display(Name = "Sipariş / Kabul")]
    Ordered = 60,

    [Display(Name = "Tamamlandı")]
    Completed = 70
}

/// <summary>
/// Görevli havuzundaki rol. Kaynak sistemde her görevli tablosu (yaklaşık
/// maliyet, piyasa araştırması, komisyon) aynı "sıra numarası + personel +
/// unvan" üçlüsünü tekrar eder; bu enum o tekrarlanan sıra numarasını
/// sabitler.
/// </summary>
public enum ProcurementTeamRole
{
    [Display(Name = "Yaklaşık Maliyet Görevlisi")]
    CostEstimation = 1,

    [Display(Name = "Piyasa Araştırması Görevlisi")]
    MarketSurvey = 2,

    [Display(Name = "Komisyon Üyesi")]
    Commission = 3,

    [Display(Name = "Komisyon Başkanı")]
    CommissionChair = 4
}