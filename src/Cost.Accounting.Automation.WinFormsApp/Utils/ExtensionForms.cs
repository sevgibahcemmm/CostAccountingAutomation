using Microsoft.Extensions.DependencyInjection;
using System.Windows.Forms;

namespace Cost.Accounting.Automation.WinFormsApp.Utils
{
    public static class ExtensionForms
    {
        public static void AddForms(this IServiceCollection services)
        {
            var formTypes = typeof(Program).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Form))
                            && !t.IsAbstract
                            && t.GetConstructor(Type.EmptyTypes) != null);

            foreach (var formType in formTypes)
            {
                services.AddTransient(formType);
            }
        }
    }
}