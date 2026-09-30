using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;

namespace Cost.Accounting.Automation.WinFormsApp.Tools
{
    /// <summary>
    /// Uzun süren veri yükleme işlemlerini <see cref="WaitForm"/> ile sarar.
    ///
    /// Uygulama genelinde bekleme penceresinin davranışı tek yerden yönetilir:
    /// aynı anda yalnızca bir bekleme penceresi açık olur, işlem bitince
    /// pencere kapanır, hata oluşursa da yine kapatılır.
    /// </summary>
    public static class LoadingHelper
    {
        private static readonly TimeSpan DefaultMinimumDuration =
            TimeSpan.FromMilliseconds(400);

        private static readonly TimeSpan DefaultCompletionDuration =
            TimeSpan.FromMilliseconds(1200);

        private static WaitForm? _current;

        /// <summary>
        /// <paramref name="action"/> çalışırken bekleme penceresi gösterir.
        /// </summary>
        /// <param name="action">Yüklenecek veriyi getiren işlem.</param>
        /// <param name="caption">Bekleme penceresi başlığı.</param>
        /// <param name="description">Bekleme penceresi açıklaması.</param>
        /// <param name="showCompleted">
        /// İşlem bittikten sonra "Tüm veriler yüklendi" onayının gösterilip
        /// gösterilmeyeceği.
        /// </param>
        /// <param name="minimumDuration">
        /// Bekleme penceresinin asgari görünür kalma süresi. Çok hızlı işlemlerde
        /// pencerenin fark edilememesini önler.
        /// </param>
        /// <param name="completionDuration">
        /// "Tüm veriler yüklendi" onayının ekranda kalma süresi.
        /// </param>
        /// <param name="completionSummary">
        /// İşlem bittikten sonra onay metninin altında gösterilecek özet satırı.
        /// </param>
        public static async Task RunAsync(
            Func<Task> action,
            string caption = "Veriler yükleniyor...",
            string description = "Lütfen bekleyin...",
            bool showCompleted = false,
            TimeSpan? minimumDuration = null,
            TimeSpan? completionDuration = null,
            Func<string>? completionSummary = null)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            // Önceki yükleme hâlâ açık bir pencere bıraktıysa kapatılır;
            // böylece ekranda üst üste binen pencereler oluşmaz.
            CloseCurrent();

            WaitForm waitForm =
                WaitFormHelper.Show<WaitForm>(caption, description);

            _current = waitForm;
            long shownAt = Environment.TickCount64;

            try
            {
                await action();

                // Bekleme penceresi çok hızlı biten işlemlerde fark edilemeyebilir;
                // asgari görünürlük süresi her zaman uygulanır.
                await EnsureMinimumVisibleAsync(
                    waitForm,
                    shownAt,
                    minimumDuration ?? DefaultMinimumDuration);

                if (showCompleted)
                {
                    // Onay, veri geldikten sonra gösterilen son bir
                    // göstergedir. DevExpress tarafından reddedilse bile veri
                    // akışı bozulmamalı ve pencere kapanmalıdır.
                    try
                    {
                        waitForm.SetCompleted(
                            "Tüm veriler yüklendi",
                            GetSummary(completionSummary));
                    }
                    catch (ArgumentException)
                    {
                    }

                    await Task.Delay(completionDuration ?? DefaultCompletionDuration);
                }
            }
            finally
            {
                Close(waitForm);
            }
        }

        /// <summary>
        /// <typeparam name="T">İşlemin döndürdüğü değer türü.</typeparam>
        /// <param name="action">Yüklenecek veriyi getiren işlem.</param>
        /// <param name="caption">Bekleme penceresi başlığı.</param>
        /// <param name="description">Bekleme penceresi açıklaması.</param>
        /// <param name="showCompleted">
        /// İşlem bittikten sonra "Tüm veriler yüklendi" onayının gösterilip
        /// gösterilmeyeceği.
        /// </param>
        public static async Task<T> RunAsync<T>(
            Func<Task<T>> action,
            string caption = "Veriler yükleniyor...",
            string description = "Lütfen bekleyin...",
            bool showCompleted = false,
            TimeSpan? minimumDuration = null,
            TimeSpan? completionDuration = null,
            Func<string>? completionSummary = null)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            CloseCurrent();

            WaitForm waitForm =
                WaitFormHelper.Show<WaitForm>(caption, description);

            _current = waitForm;
            long shownAt = Environment.TickCount64;

            try
            {
                T result = await action();

                await EnsureMinimumVisibleAsync(
                    waitForm,
                    shownAt,
                    minimumDuration ?? DefaultMinimumDuration);

                if (showCompleted)
                {
                    // Onay, veri geldikten sonra gösterilen son bir
                    // göstergedir. DevExpress tarafından reddedilse bile veri
                    // akışı bozulmamalı ve pencere kapanmalıdır.
                    try
                    {
                        waitForm.SetCompleted(
                            "Tüm veriler yüklendi",
                            GetSummary(completionSummary));
                    }
                    catch (ArgumentException)
                    {
                    }

                    await Task.Delay(completionDuration ?? DefaultCompletionDuration);
                }

                return result;            }
            finally
            {
                Close(waitForm);
            }
        }

        /// <summary>Açık bekleme penceresini kapatır.</summary>
        public static void CloseCurrent()
        {
            if (_current is null)
            {
                return;
            }

            Close(_current);
        }

        /// <summary>
        /// Bekleme penceresi asgari süre boyunca görünür kalır.
        /// </summary>
        private static async Task EnsureMinimumVisibleAsync(
            WaitForm waitForm,
            long shownAt,
            TimeSpan minimumDuration)
        {
            long elapsed = Environment.TickCount64 - shownAt;

            if (elapsed < minimumDuration.TotalMilliseconds)
            {
                await Task.Delay(
                    (int)(minimumDuration.TotalMilliseconds - elapsed));
            }
        }

        /// <summary>
        /// Tamamlanma özetini üretir; özet üretimi başarısız olursa
        /// onay metni yine de gösterilir.
        /// </summary>
        private static string GetSummary(Func<string>? completionSummary)
        {
            if (completionSummary is null)
            {
                return string.Empty;
            }

            try
            {
                return completionSummary() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static void Close(WaitForm waitForm)
        {
            if (ReferenceEquals(_current, waitForm))
            {
                _current = null;
            }

            if (waitForm.IsDisposed)
            {
                return;
            }

            waitForm.Close();
            waitForm.Dispose();
        }
    }
}
