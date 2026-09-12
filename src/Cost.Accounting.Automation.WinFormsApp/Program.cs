using Cost.Accounting.Automation.Application;
using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Infrastructure;
using Cost.Accounting.Automation.Infrastructure.Services;
using Cost.Accounting.Automation.WinFormsApp.Forms;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using Cost.Accounting.Automation.WinFormsApp.Tools;
using Cost.Accounting.Automation.WinFormsApp.Utils;
using DevExpress.LookAndFeel;
using DevExpress.UserSkins;
using DevExpress.XtraEditors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace Cost.Accounting.Automation.WinFormsApp
{
    internal static class Program
    {
        public static IServiceProvider Services { get; set; } = default!;

        private static void InstallCrashLogHandlers()
        {
            System.Windows.Forms.Application.ThreadException += (s, e) => CrashLog.WriteException("ThreadException", e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                CrashLog.WriteException("Unhandled" + (e.IsTerminating ? "(Terminating)" : ""), e.ExceptionObject as Exception ?? new Exception(e.ExceptionObject?.ToString()));
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
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
            WindowsFormsSettings.DefaultLookAndFeel.SetSkinStyle(SkinStyle.Sharp);

            ApplicationConfiguration.Initialize();

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            ServiceCollection services = new();
            services.AddSingleton(configuration);
            services.AddApplication();
            services.AddInfrastructure(configuration);

            services.AddSingleton<SessionClaimContext>();
            services.AddSingleton<IClaimContext>(sp => sp.GetRequiredService<SessionClaimContext>());

            services.AddForms();

            Services = services.BuildServiceProvider();

            InstallCrashLogHandlers();
            InstallSessionFileLogging();
DatabaseInitializer.InitializeAsync(Services).GetAwaiter().GetResult();

            var loginForm = Services.GetRequiredService<XtraLoginForm>();
            System.Windows.Forms.Application.Run(loginForm);
        }
    }
}