namespace Cost.Accounting.Automation.Application;

/// <summary>
/// Silme komutlarının "uyar ama sil" davranışında kullandığı mesaj öneki.
/// Başarılı sonuç bu önekle başlarsa CrudExecutor uyarı (Warning) toast'ı gösterir.
/// </summary>
public static class DeleteWarnings
{
    public const string Prefix = "UYARI|";

    public static string Compose(string message) => Prefix + message;
}