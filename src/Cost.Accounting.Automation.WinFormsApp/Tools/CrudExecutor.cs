using Cost.Accounting.Automation.Application;
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
            Result<T>? result = await TryExecuteAsync(request);
            return result is not null;
        }

        public static async Task<Result<T>?> TryExecuteAsync<T>(IRequest<Result<T>> request)
        {
            try
            {
                using var scope = Program.Services.CreateScope();
                ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();
                Result<T> result = await mediator.Send(request, CancellationToken.None);

                if (!result.IsSuccessful)
                {
                    ToastHelper.Show(AuthFormStyles.GetErrorText(result.ErrorMessages), ToastType.Error, 4000);
                    return null;
                }

                string message = GetResultMessage(result.Data);
                if (message.StartsWith(DeleteWarnings.Prefix, StringComparison.Ordinal))
                {
                    ToastHelper.Show(message[DeleteWarnings.Prefix.Length..], ToastType.Warning, 5000);
                }
                else
                {
                    ToastHelper.Show(message, ToastType.Success);
                }
                return result;
            }
            catch (ValidationException ex)
            {
                ToastHelper.Show(AuthFormStyles.GetValidationText(ex), ToastType.Error, 4000);
                return null;
            }
            catch (Exception ex)
            {
                CrashLog.WriteException("CrudExecutorCatch-" + request.GetType().Name, ex);
                ToastHelper.Show("İşlem sırasında bir hata oluştu: " + ex.Message, ToastType.Error, 4000);
                return null;
            }
        }

        private static string GetResultMessage(object? data) => data switch
        {
            string s => s,
            IResultMessage m => m.Message,
            _ => "İşlem başarılı"
        };
    }
}