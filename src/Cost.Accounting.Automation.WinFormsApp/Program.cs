using Cost.Accounting.Automation.Application;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Infrastructure;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.LookAndFeel;
using DevExpress.UserSkins;
using DevExpress.XtraEditors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        /// Giriş ekranından önce veritabanının hazır olduğundan emin olur.
        ///
        /// <para>
        /// <b>Veritabanı oluşmuşsa kurulum penceresi hiç açılmaz.</b> Bekleyen
        /// migration, tohumlama ve yıl veritabanı işlemleri
        /// <see cref="DatabaseInitializer.InitializeAsync"/> tarafından sessizce
        /// ve idempotent olarak yapılır; ekrana hiçbir şey çıkmaz. Kullanıcı
        /// her açılışta kısa süreli bir kurulum penceresi görmez.
        /// </para>
        ///
        /// <para>
        /// Pencere yalnızca <b>ilk kurulumda</b> veya sunucuya ulaşılamadığında
        /// gereklidir: veritabanı yoksa oluşturulması, sunucu kapalıysa hatanın
        /// ve "Yeniden Dene" düğmesinin gösterilmesi ekran gerektirir. Bu
        /// durumlarda pencere tüm veritabanı işini kendi içinde yapar.
        /// </para>
        ///
        /// <c>true</c> dönerse giriş ekranı açılabilir.
        /// </summary>
        private static bool PrepareDatabaseBeforeLogin()
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
