using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Cost.Accounting.Automation.Application.Auth;
using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Application.ChartOfAccounts;
using Cost.Accounting.Automation.Application.Services;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application;
public static class ServiceRegistrar
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<PermissionService>();
        services.AddSingleton<ICaptchaService, MathCaptchaService>();

        services.AddScoped<IChartOfAccountLedgerPoster, ChartOfAccountLedgerPoster>();

        services.AddMediatR(cfr =>
        {
            cfr.RegisterServicesFromAssembly(typeof(ServiceRegistrar).Assembly);

            // 1. Önce Loglama (İstek girdi mi? Hata var mı?)
            cfr.AddOpenBehavior(typeof(LoggingBehavior<,>));

            // 2. Sonra Doğrulama (Veri geçerli mi? Değilse geri dön)
            cfr.AddOpenBehavior(typeof(ValidationBehavior<,>));

            // 3. Sonra Yetki (Kullanıcının yetkisi var mı?)
            cfr.AddOpenBehavior(typeof(PermissionBehavior<,>));

            // 4. En son Transaction (Her şey tamsa DB işlemini yönet ve kaydet)
            cfr.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(ServiceRegistrar).Assembly);

        return services;
    }
}
