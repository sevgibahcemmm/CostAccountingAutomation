using Cost.Accounting.Automation.Application;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Infrastructure;
using Cost.Accounting.Automation.Infrastructure.Options;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.LookAndFeel;
using DevExpress.UserSkins;
using DevExpress.XtraEditors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace Cost.Accounting.Automation.WinFormsApp
{
    internal static class Program
    {
        public static IServiceProvider Services { get; set; } = default!;

        private static void InstallCrashLogHandlers()
        {
            System.Windows.Forms.Application.ThreadException += (s, e) => {
                CrashLog.WriteException("ThreadException", e.Exception);
                Console.Error.WriteLine("[THREAD] " + e.Exception.Message);
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) => {
                var ex = e.ExceptionObject as Exception ?? new Exception(e.ExceptionObject?.ToString() ?? "Unknown");
                CrashLog.WriteException("Unhandled" + (e.IsTerminating ? "(Terminating)" : ""), ex);
                Console.Error.WriteLine("[UNHANDLED" + (e.IsTerminating ? "(Terminating)" : "") + "] " + ex.Message + "\n" + ex.StackTrace);
            };
            TaskScheduler.UnobservedTaskException += (s, e) => {
                CrashLog.WriteException("UnobservedTask", e.Exception);
                e.SetObserved();
            };
        }

        private static void InstallSessionFileLogging()
        {
            try
            {
                string dir = Path.Combine(AppContext.BaseDirectory, "logs");
                Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, "crash.log"), string.Empty);
                CrashLog.Write("Session", "Application started: " + Environment.Version);
            }
            catch
            {
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            DevExpressLocalizers.Register();

            BonusSkins.Register();
            WindowsFormsSettings.DefaultLookAndFeel.SetSkinStyle(SkinStyle.DarkSide);

            ApplicationConfiguration.Initialize();

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                // Makineye ozgu, git tarafindan izlenmeyen ayarlar. Kurulumda bu
                // dosya klasorle birlikte kopyalanir; kullanici hicbir sey yapmaz.
                // Git tarafindan ignore edilir (.gitignore).
                .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
                // En yuksek oncelik: ortam degiskenleri. Nokta ile ayri anahtar
                // hiyerarsisi kullanilir: ConnectionStrings__Master, Jwt__SecretKey,
                // DatabaseProvisioning__Mode.
                .AddEnvironmentVariables()
                .Build();

            ServiceCollection services = new();
            services.AddSingleton(configuration);
            services.AddApplication();
            services.AddInfrastructure(configuration);
            services.AddLogging();

            services.AddSingleton<SessionClaimContext>();
            services.AddSingleton<IClaimContext>(sp => sp.GetRequiredService<SessionClaimContext>());

            services.AddForms();

            Services = services.BuildServiceProvider();

            InstallCrashLogHandlers();
            InstallSessionFileLogging();

            if (!EnsureSecretsConfigured())
            {
                return;
            }

            if (!PrepareDatabaseBeforeLogin())
            {
                return;
            }

            var loginForm = Services.GetRequiredService<XtraLoginForm>();
            CrashLog.Write("Main", "After loginForm");
            CrashLog.Write("Main", "Before Application.Run");
            try
            {
                System.Windows.Forms.Application.Run(loginForm);
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("Main.ApplicationRun", ex);
                Console.Error.WriteLine("[RUN] " + ex.Message);
                throw;
            }
            CrashLog.Write("Main", "After Application.Run");
        }

        /// <summary>
        /// Gizli değerlerin (JWT imzalama anahtarı) tanımlı olduğunu doğrular.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Anahtar <c>appsettings.json</c>'da bulunmaz; ortam değişkeninden gelir.
        /// Eksikliği ilk giriş denemesinde anlaşılırsa kullanıcı yalnızca "geçersiz
        /// kullanıcı adı" görür ve gerçek nedeni bulamaz. Bu yüzden kontrol giriş
        /// ekranından önce yapılır ve pencere yerine düz bir mesaj kutusu gösterilir.
        /// </para>
        /// </remarks>
        /// <returns>Yapılandırma tamamsa <c>true</c>.</returns>
        private static bool EnsureSecretsConfigured()
        {
            try
            {
                Services.GetRequiredService<IOptions<JwtOptions>>().Value.EnsureConfigured();

                return true;
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("Main.EnsureSecretsConfigured", ex);

                MessageBox.Show(
                    ex.Message,
                    "Yapılandırma eksik",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return false;
            }
        }

        /// <summary>
        /// Giriş ekranından önce veritabanının uygulamaya hazır olduğundan emin olur.
        ///
        ///
        /// <para>
        /// Davranış <c>DatabaseProvisioning:Mode</c> ayarına göre ikiye ayrılır:
        /// </para>
        /// <list type="bullet">
        /// <item>
        /// <b>VerifyOnly</b> (varsayılan, merkezi sunucu): uygulama veritabanına
        /// hiç dokunmaz. Yalnızca şemanın güncel olduğunu salt okunur doğrular.
        /// Eksikse <see cref="DatabaseNotReadyForm"/> gösterilir ve giriş engellenir.
        /// </item>
        /// <item>
        /// <b>Automatic</b> (yalnızca tek geliştirici / LocalDB): uygulama veritabanını
        /// kendisi hazırlar; yoksa kurulum sihirbazı gösterilir.
        /// </item>
        /// </list>
        ///
        /// <para>
        /// Ayrım zorunludur. Merkezi sunucuya bağlanan on istemci aynı anda açıldığında
        /// her biri migration çalıştırıp tohumlama yaparsa migration geçmişi tablosunda
        /// çakışma ve çift kayıt oluşur. Şemayı yalnızca yönetici hazırlar
        /// (<c>caa-provision provision</c>); istemciler yalnızca okur.
        /// </para>
        /// </summary>
        /// <returns>
        /// <c>true</c> dönerse giriş ekranı açılabilir.
        /// </returns>
        private static bool PrepareDatabaseBeforeLogin()
        {
            DatabaseProvisioningMode mode = ResolveProvisioningMode();

            return mode == DatabaseProvisioningMode.Automatic
                ? PrepareDatabaseAutomatically()
                : VerifyDatabaseWithoutWriting();
        }

        /// <summary>
        /// Kurulum modu ayarını okur. Hatalı bir değer yazılmışsa uygulama
        /// güvenli tarafa düşer: <see cref="DatabaseProvisioningMode.VerifyOnly"/>.
        /// </summary>
        /// <remarks>
        /// "Okunamayan ayar = şemaya dokunma" kuralı bilinçlidir. Bir yazım
        /// hatası (örn. "Automatic" yerine "auto") istemcinin sunucuda şema
        /// değiştirmesine yol açmamalıdır.
        /// </remarks>
        private static DatabaseProvisioningMode ResolveProvisioningMode()
        {
            string? raw = Services.GetRequiredService<IConfiguration>()
                .GetValue<string>($"{DatabaseProvisioningOptions.SectionName}:Mode");

            return Enum.TryParse(raw, ignoreCase: true, out DatabaseProvisioningMode mode)
                ? mode
                : DatabaseProvisioningMode.VerifyOnly;
        }

        /// <summary>
        /// <b>VerifyOnly</b> modu: hiçbir şey yazmadan şemanın güncel olduğunu
        /// doğrular. Kullanıcı "Yeniden Dene" dediğinde kontrol tekrarlanır.
        /// </summary>
        private static bool VerifyDatabaseWithoutWriting()
        {
            while (true)
            {
                DatabaseSchemaCheckResult result = DatabaseInitializer
                    .CheckSchemaAsync(Services)
                    .GetAwaiter()
                    .GetResult();

                CrashLog.Write("Main", $"Sema kontrolu: {result.State}");

                if (result.IsReady)
                {
                    return true;
                }

                using var notReadyForm = new DatabaseNotReadyForm(result);

                if (notReadyForm.ShowDialog() != DialogResult.Retry)
                {
                    CrashLog.Write("Main", "Sema hazir degil; giris ekrani acilmadi.");

                    return false;
                }
            }
        }

        /// <summary>
        /// <b>Automatic</b> modu: uygulama veritabanını kendisi hazırlar.
        ///
        /// <para>
        /// Veritabanı zaten hazırsa kullanıcı hiçbir şey görmez. Yoksa kurulum
        /// sihirbazı açılır. Hazırlık işlemi <c>sp_getapplock</c> ile kilitli
        /// olduğundan aynı anda iki geliştirici uygulamayı açarsa ikincisi
        /// bekler ve sonunda "kurulum zaten tamamlanmış" görür.
        /// </para>
        /// </summary>
        private static bool PrepareDatabaseAutomatically()
        {
            if (TryPrepareExistingDatabaseSilently())
            {
                CrashLog.Write("Main", "Database exists and is ready; skipping setup form.");

                return true;
            }

            using var setupForm = new DatabaseSetupForm();

            System.Windows.Forms.Application.Run(setupForm);

            if (!setupForm.ShouldContinueToLogin)
            {
                CrashLog.Write("Main", "Database setup did not complete; exiting before login.");

                return false;
            }

            CrashLog.Write("Main", "Database ready; opening login.");

            return true;
        }

        /// <summary>
        /// Veritabanı oluşmuşsa pencere açmadan hazırlar.
        ///
        /// <para>
        /// <see cref="DatabaseInitializer.GetFirstRunStateAsync"/> yalnızca varlık
        /// sorar ve kısa zaman aşımı kullanır; yoklama başarısız olursa ya da
        /// veritabanı yoksa <c>false</c> döner ve kurulum penceresi devreye
        /// girer. Veritabanı varsa <see cref="DatabaseInitializer.InitializeAsync"/>
        /// çağrılır; bu metot zaten idempotenttir, veriler hazırsa hiçbir şey
        /// yazmaz.
        /// </para>
        ///
        /// <para>
        /// Beklenmedik bir hata da <c>false</c> döner: sessizce geçip bozuk bir
        /// şema üzerinde açmak yerine pencerenin hatayı göstermesi tercih edilir.
        /// </para>
        /// </summary>
        private static bool TryPrepareExistingDatabaseSilently()
        {
            try
            {
                DatabaseFirstRunState state = DatabaseInitializer
                    .GetFirstRunStateAsync(Services)
                    .GetAwaiter()
                    .GetResult();

                if (state != DatabaseFirstRunState.Exists)
                {
                    return false;
                }

                DatabaseInitializer
                    .InitializeAsync(Services)
                    .GetAwaiter()
                    .GetResult();

                return true;
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("Main.PrepareExistingDatabase", ex);

                return false;
            }
        }
    }
}
