using System.ComponentModel.DataAnnotations;

namespace Cost.Accounting.Automation.Domain.CostSlips;

public enum CostSlipType : byte
{
    [Display(Name = "Hizmet Maliyet Pusulası", Description = "Hizmet bazlı maliyetler için")]
    Service = 1,

    [Display(Name = "Mamul Maliyet Pusulası", Description = "Mamul/Ürün bazlı maliyetler için")]
    Product = 2,

    [Display(Name = "Yarı Mamul Maliyet Pusulası", Description = "Yarı Mamul/Ürün bazlı maliyetler için")]
    SemiFinishedProduct = 3
}