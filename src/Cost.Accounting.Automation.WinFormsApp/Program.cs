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
            WindowsFormsSettings.DefaultLookAndFeel.SetSkinStyle(SkinStyle.Office2019Black);

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
        /// Açılış penceresi HER ZAMAN gösterilir ve tüm veritabanı işini kendi
        /// içinde yapar: yoklama, ilk kurulum ve şema güncellemesi. Böylece
        /// pencere açılmadan önce hiçbir ağ/sunucu beklemesi olmaz; kullanıcı
        /// donmuş bir ekran görmez.
        ///
        /// Veritabanı zaten güncelse pencere kısa süre sonra kendini kapatır ve
        /// giriş ekranı açılır; yoksa adımlar tik işaretleriyle gösterilir.
        ///
        /// <c>true</c> dönerse giriş ekranı açılabilir.
        /// </summary>
        private static bool PrepareDatabaseBeforeLogin()
        {
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
    }
}
