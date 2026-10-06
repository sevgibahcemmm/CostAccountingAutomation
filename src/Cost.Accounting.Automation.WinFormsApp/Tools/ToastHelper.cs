using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;

namespace Cost.Accounting.Automation.WinFormsApp.Tools
{
    public static class ToastHelper
    {
        /// <summary>
        /// Bildirim baloncuklarının çıkacağı ekran noktasını üreten sağlayıcı.
        /// </summary>
        /// <remarks>
        /// RibbonMainForm bunu durum çubuğundaki çevrimiçi göstergesine bağlar:
        /// "X oturum açtı" bildirimi, kullanıcının dikkatinin zaten olduğu
        /// köşeden yükselir ve sesle desteklenir. Sağlayıcı hata verirse (örneğin
        /// ana form kapanıyorsa) konum sessizce yok sayılır ve bildirim köşeden
        /// açılır.
        /// </remarks>
        public static Func<Point?>? AnchorProvider { get; set; }

        /// <summary>
        /// "X size mesaj gönderdi" bildirimine basıldığında çalışır; ilgili
        /// konuşmayı doğrudan açar. RibbonMainForm tarafından bağlanır.
        /// </summary>
        public static Action<Guid, string>? OpenConversation { get; set; }

        /// <summary>
        /// Oturum açma/kapanış gibi genel bildirimlere basıldığında çalışır;
        /// Mesajlar sayfasını açar. RibbonMainForm tarafından bağlanır.
        /// </summary>
        public static Action? OpenMessages { get; set; }

        public static void Show(
            string message,
            ToastType type = ToastType.Info,
            int durationMs = 3000,
            Action? onClick = null)
        {
            Point? anchor = null;

            try
            {
                anchor = AnchorProvider?.Invoke();
            }
            catch
            {
                // Konum alınamadıysa köşedeki varsayılan davranışa düşülür.
            }

            ToastForm toast = new();
            toast.ShowToast(message, type, durationMs, anchor, onClick);
        }
    }
}