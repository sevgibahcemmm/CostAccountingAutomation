using System.ComponentModel.DataAnnotations;

namespace Cost.Accounting.Automation.Domain.Invoices;

public enum InvoiceType
{
    [Display(Name = "Alış Faturası")]
    Purchase = 1,
    [Display(Name = "Satış Faturası")]
    Sales = 2,
    [Display(Name = "Alışlardan İade Faturası")]
    PurchaseReturn = 3,
    [Display(Name = "Satışlardan İade Faturası")]
    SalesReturn = 4
}