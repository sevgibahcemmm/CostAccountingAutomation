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
        /// <summary>
        /// "Tüm veriler yüklendi" onayının ekranda kalma süresi.
        ///
        /// <para>
        /// Bu süre <b>yapay gecikmedir</b>: veri çoktan yüklenmiş ve ekrana
        /// uygulanmıştır. Amaç yalnızca "işlem bitti" bilgisini bir an
        /// görünür kılmaktır. Bu yüzden bilinçli olarak kısa tutulur; uzun bir
        /// süre, kullanıcının hazır olan veriye bakmasını engeller.
        /// </para>
        ///
        /// <para>
        /// Ölçüm notu: dashboard verisi 9 ms'de gelip çizimi 7 ms'de tamamlanıyor,
        /// ancak 1200 ms'lik onay süresi kullanıcıya ~1,4 sn beklettiriyordu —
        /// bekleme süresinin %98'i gerçek iş değil, bu gecikmeden geliyordu.
        /// </para>
        /// </summary>
        private static readonly TimeSpan DefaultCompletionDuration =
            TimeSpan.FromMilliseconds(350);

        /// <summary>
        /// Bekleme penceresinin gösterileceği asgari eşik. Daha hızlı biten
        /// işlerde pencere hiç açılmaz; kullanıcı titreme görmez, veri de anında
        /// görünür.
        /// </summary>
        private static readonly TimeSpan ShowThreshold = TimeSpan.FromMilliseconds(150);

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

            // Hızlı biten işlerde (küçük listeler) bekleme penceresi hiç
            // gösterilmez: kullanıcı ne titreme ne de gereksiz "bekleyin"
            // görür, veri anında dolar. Pencere yalnızca iş eşiği aşarsa
            // gösterilir; böylece gerçekten yavaş sorguların kullanıcısı nerede
            // takıldığını görür.
            Task actionTask = action();
            WaitForm? waitForm = null;

            if (!actionTask.IsCompleted)
            {
                Task finished = await Task.WhenAny(
                    actionTask,
                    Task.Delay(ShowThreshold));

                if (finished != actionTask)
                {
                    waitForm = ShowWaitForm(caption, description);
                }
            }

            try
            {
                await actionTask;

                // Onay YALNIZCA kullanıcı gerçekten "yükleniyor" penceresini
                // gördüyse gösterilir.
                //
                // Yükleme ShowThreshold (150 ms) altında bittiyse bekleme
                // penceresi hiç açılmamış demektir; veri zaten ekrandadır. Bu
                // durumda onay göstermek, kullanıcıya hiçbir şey anlatmayan
                // yapay bir pencere açıp onu bekletmekten başka bir işe
                // yaramaz.
                if (showCompleted && waitForm is not null)
                {
                    // DevExpress tarafından reddedilse bile veri akışı
                    // bozulmamalı ve pencere kapanmalıdır.
                    try
                    {
                        waitForm.SetCompleted("Tüm veriler yüklendi", GetSummary(completionSummary));
                    }
                    catch (Exception ex)
                    {
                        CrashLog.WriteException("LoadingHelper.ShowCompletion", ex);
                    }

                    await Task.Delay(completionDuration ?? DefaultCompletionDuration);
                }
            }
            finally
            {
                if (waitForm is not null)
                {
                    Close(waitForm);
                }
            }
        }

        /// <summary>Bekleme penceresini açar ve aktif olarak işaretler.</summary>
        private static WaitForm ShowWaitForm(string caption, string description)
        {
            WaitForm waitForm = WaitFormHelper.Show<WaitForm>(caption, description);
            _current = waitForm;
            return waitForm;
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
            TimeSpan? completionDuration = null,
            Func<string>? completionSummary = null)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            CloseCurrent();

            Task<T> actionTask = action();
            WaitForm? waitForm = null;

            if (!actionTask.IsCompleted)
            {
                Task finished = await Task.WhenAny(
                    actionTask,
                    Task.Delay(ShowThreshold));

                if (finished != actionTask)
                {
                    waitForm = ShowWaitForm(caption, description);
                }
            }

            try
            {
                T result = await actionTask;

                // Bkz. RunAsync(Func<Task>) — hızlı biten yüklemelerde bekleme
                // penceresi hiç açılmadıysa onay gösterilmez.
                if (showCompleted && waitForm is not null)
                {
                    try
                    {
                        waitForm.SetCompleted("Tüm veriler yüklendi", GetSummary(completionSummary));
                    }
                    catch (Exception ex)
                    {
                        CrashLog.WriteException("LoadingHelper.ShowCompletion", ex);
                    }

                    await Task.Delay(completionDuration ?? DefaultCompletionDuration);
                }

                return result;
            }
            finally
            {
                if (waitForm is not null)
                {
                    Close(waitForm);
                }
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

            // Pencere her seferinde yeniden çözülüp Dispose edildiği için
            // (AddForms kaydı Transient) z-sırası ayarının sıfırlanmasına
            // gerek yoktur.
            waitForm.Close();
            waitForm.Dispose();
        }
    }
}
