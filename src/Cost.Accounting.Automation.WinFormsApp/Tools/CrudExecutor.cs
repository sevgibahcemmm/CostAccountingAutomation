using Cost.Accounting.Automation.WinFormsApp.Forms.BaseForm;
using Cost.Accounting.Automation.WinFormsApp.Forms.MainForms;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;
using TS.Result;

namespace Cost.Accounting.Automation.WinFormsApp.Tools
{
    public static class CrudExecutor
    {
        public static async Task<bool> ExecuteAsync<T>(IRequest<Result<T>> request)
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                Result<T> result = await mediator.Send(request, CancellationToken.None);

                if (!result.IsSuccessful)
                {
                    ToastHelper.Show(AuthFormStyles.GetErrorText(result.ErrorMessages), ToastType.Error, 4000);
                    return false;
                }

                string message = result.Data is string s ? s : "İşlem başarılı";
                ToastHelper.Show(message, ToastType.Success);
                return true;
            }
            catch (ValidationException ex)
            {
                ToastHelper.Show(AuthFormStyles.GetValidationText(ex), ToastType.Error, 4000);
                return false;
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("CrudExecutorCatch-" + request.GetType().Name, ex);
                ToastHelper.Show("İşlem sırasında bir hata oluştu: " + ex.Message, ToastType.Error, 4000);
                return false;
            }
        }
    }
}