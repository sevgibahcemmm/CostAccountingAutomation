namespace Cost.Accounting.Automation.Domain.CurrentAccounts;

public enum CurrentAccountMovementType
{
    SalesInvoice = 1,    // Satış Faturası (Borç)
    PurchaseInvoice = 2, // Satın Alma Faturası (Alacak)
    Collection = 3,      // Tahsilat (Müşteriden tahsilat -> Alacak)
    Payment = 4,         // Ödeme (Tedarikçiye ödeme -> Borç)
    OpeningBalance = 5,  // Açılış / Devir Bakiyesi
    DebitVoucher = 6,    // Borç Dekontu
    CreditVoucher = 7    // Alacak Dekontu
}
