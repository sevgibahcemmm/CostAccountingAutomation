using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;

namespace Cost.Accounting.Automation.WinFormsApp.Tools
{
    public static class ToastHelper
    {
        public static void Show(string message, ToastType type = ToastType.Info, int durationMs = 3000)
        {
            ToastForm toast = new();
            toast.ShowToast(message, type, durationMs);
        }
    }
}