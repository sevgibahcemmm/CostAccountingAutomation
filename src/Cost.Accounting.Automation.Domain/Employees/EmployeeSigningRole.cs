using System.ComponentModel.DataAnnotations;

namespace Cost.Accounting.Automation.Domain.Employees;

/// <summary>
/// Bir belgenin imza bölümünü dolduran yetkili görevi.
///
/// Raporların imza alanları bu değerlere göre çözülür: her görev için bir
/// <see cref="EmployeeDuty"/> kaydı aranır ve o kaydın personeli imza satırına
/// adı soyadı ve ünvanı ile basılır.
/// </summary>
public enum EmployeeSigningRole : byte
{
    [Display(Name = "Sabit Görevli", Description = "Belgeyi düzenleyen ve teslim eden sabit kadro görevlisi")]
    PermanentOfficer = 1,

    [Display(Name = "Atölye Şefi", Description = "Üretimin yapıldığı atölyenin şefi; mamul beyanı ve maliyet pusulası imzası")]
    WorkshopChief = 2,

    [Display(Name = "Taşınır Kayıt Yetkilisi", Description = "Taşınır işlem fişi giriş/çıkış kaydını yapan yetkili")]
    MovableAssetOfficer = 3,

    [Display(Name = "İşyurdu Müdürü", Description = "Kurumun en üst yetkilisi; maliyet pusulası onay imzası")]
    InstitutionDirector = 4,

    [Display(Name = "Muhasebe Yetkilisi", Description = "Mali kayıtları tutan ve onaylayan muhasebe yetkilisi")]
    AccountingOfficer = 5,

    [Display(Name = "Harcama Yetkilisi", Description = "Harcama onaylayan yetkili")]
    SpendingOfficer = 6,

    [Display(Name = "Sayım Yapan", Description = "Stok sayımını fiilen gerçekleştiren personel")]
    Counter = 7,

    [Display(Name = "Kontrol Eden", Description = "Sayım sonucunu kontrol edip onaylayan personel")]
    Controller = 8
}