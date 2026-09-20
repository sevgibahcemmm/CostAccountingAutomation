namespace Cost.Accounting.Automation.WinFormsApp.Reports
{
    public sealed class GiderDagitimRow
    {
        public int SiraNo { get; set; }
        public string HesapAdi { get; set; } = string.Empty;
        public decimal Tutar { get; set; }
        public decimal Oran { get; set; }
    }
}