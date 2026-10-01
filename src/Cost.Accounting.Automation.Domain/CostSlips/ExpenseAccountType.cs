using System.ComponentModel.DataAnnotations;

namespace Cost.Accounting.Automation.Domain.CostSlips;

public enum ExpenseAccountType : byte
{
    [Display(Name = "710 - Direkt İlk Madde ve Malzeme Giderleri", Description = "Mamul")]
    Account710 = 1,

    [Display(Name = "720-01 - Direkt İşçilik Giderleri", Description = "Mamul")]
    Account720_1 = 2,

    [Display(Name = "720-02 - Sosyal Güvenlik Kurumlarına Devlet Primi Giderleri", Description = "Mamul")]
    Account720_2 = 3,

    [Display(Name = "730-01 - Endirek İlk Madde ve Malzeme Gideri Hesabı", Description = "Mamul")]
    Account730_01 = 4,

    [Display(Name = "730-02 - Üretimle İlgili Dışarıya Yaptırılan İşler", Description = "Mamul")]
    Account730_02 = 5,

    [Display(Name = "730-03 - İşçi Ücret ve Giderleri Hesabı", Description = "Mamul")]
    Account730_03 = 6,

    [Display(Name = "730-04 - Dışarıdan Sağlanan Fayda ve Hizmetler", Description = "Mamul")]
    Account730_04 = 7,

    [Display(Name = "730-05 - Çeşitli Giderler Hesabı", Description = "Mamul")]
    Account730_05 = 8,

    [Display(Name = "730-06 - Vergi, Resim ve Harçlar Hesabı", Description = "Mamul")]
    Account730_06 = 9,

    [Display(Name = "730-07 - Amortisman Giderleri", Description = "Mamul")]
    Account730_07 = 10,

    /// <summary>
    /// / 740-HİZMET
    /// </summary>
    [Display(Name = "740-01 - İlk Madde ve Malzeme", Description = "Hizmet")]
    Account740_1 = 11,

    [Display(Name = "740-02 - Üretimle İlgili Dışarıya Yaptırılan İşler", Description = "Hizmet")]
    Account740_2 = 12,

    [Display(Name = "740-03-01 - İşçi Ücret ve Giderleri Hesabı", Description = "Hizmet")]
    Account740_3_01 = 13,

    [Display(Name = "740-03-02 - Sosyal Güvenlik Kurumlarına Devlet Primi Giderleri", Description = "Hizmet")]
    Account740_3_02 = 14,

    [Display(Name = "740-04 - Dışarıdan Sağlanan Fayda ve Hizmetler", Description = "Hizmet")]
    Account740_4 = 15,

    [Display(Name = "740-05 - Çeşitli Giderler Hesabı", Description = "Hizmet")]
    Account740_5 = 16,

    [Display(Name = "740-06 - Vergi, Resim ve Harçlar Hesabı", Description = "Hizmet")]
    Account740_6 = 17,

    [Display(Name = "740-07 - Amortisman Giderleri", Description = "Hizmet")]
    Account740_7 = 18,

    [Display(Name = "750 - Araştırma ve Geliştirme Giderleri", Description = "Ortak")]
    Account750 = 19,

    [Display(Name = "760 - Pazarlama, Satış ve Dağıtım Giderleri", Description = "Ortak")]
    Account760 = 20,

    [Display(Name = "770 - Genel Yönetim Giderleri", Description = "Ortak")]
    Account770 = 21,

    [Display(Name = "780 - Finansman Giderleri", Description = "Ortak")]
    Account780 = 22,

    /// <summary>
    /// Yarı mamul stoğundan (151) tüketilen malzemenin maliyetidir.
    ///
    /// GetFilteredAccounts bu hesabı slip tipinden bağımsız olarak HER
    /// pusulaya ekler; bu yüzden Description değeri tip filtresinde kullanılmaz.
    /// </summary>
    [Display(Name = "151 - Yarı Mamul Stoktan Tüketilen Malzeme", Description = "Tüm")]
    Account151_SemiFinished = 23
}