using System.Diagnostics;
using Cost.Accounting.Automation.WinFormsApp.Reports.MovableAssetTransactionSlips;
using CAA = Cost.Accounting.Automation.WinFormsApp;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        var data = new MovableAssetTransactionSlipData
        {
            DocumentNumber = "000001", Date = new DateTime(2026, 10, 2) };

        // Belgeyi gercekten zorlayacak kadar kalem koy.
        for (int i = 0; i < 400; i++)
        {
            data.Rows.Add(new MovableAssetTransactionSlipRow
            { Code = "150.98.12." + (i % 90 + 10), Adi = "Kalem " + i, Quantity = i + 1, UnitPrice = 10, Amount = (i + 1) * 10 });
        }
        data.Prepare();

        var report = new CAA.Forms.Reports.MovableAssetTransactionSlipReport(data);

        // Mevcut kullanim: () => report.CreateDocumentAsync(...)
        var sw = Stopwatch.StartNew();
        Task t = report.CreateDocumentAsync();
        long donus = sw.ElapsedMilliseconds;
        bool tamamlandi = t.IsCompleted;
        sw.Stop();

        Console.WriteLine("CreateDocumentAsync() cagrisi " + donus + " ms sonra dondu");
        Console.WriteLine("  -> Task daha tamamlanmis mi : " + tamamlandi);
        Console.WriteLine();

        if (tamamlandi)
        {
            Console.WriteLine("SONUC: islem UI is parcaciginda BESINCE blokladi.");
            Console.WriteLine("        LoadingHelper actionTask.IsCompleted gordugu icin");
            Console.WriteLine("        bekleme penceresini HIC gosteremez.");
        }
        else
        {
            Console.WriteLine("SONUC: islem hemen yeni bir Task dondurdu, bekleme penceresi calisir.");
        }

        t.GetAwaiter().GetResult();
        return 0;
    }
}
