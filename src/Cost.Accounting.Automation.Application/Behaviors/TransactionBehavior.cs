

using Cost.Accounting.Automation.Application.Services;
using GenericRepository;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TS.MediatR;
using TS.Result; // Result yapınızın namespace'i

namespace Cost.Accounting.Automation.Application.Behaviors
{
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMasterUnitOfWork _masterUnitOfWork;
        private readonly IConcurrencyConflictResolver _conflictResolver;

        public TransactionBehavior(
            IUnitOfWork unitOfWork,
            IMasterUnitOfWork masterUnitOfWork,
            IConcurrencyConflictResolver conflictResolver)
        {
            _unitOfWork = unitOfWork;
            _masterUnitOfWork = masterUnitOfWork;
            _conflictResolver = conflictResolver;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var response = await next();

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                if (_masterUnitOfWork.HasChanges())
                {
                    await _masterUnitOfWork.SaveChangesAsync(cancellationToken);
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Kayıt bizim yazdığımızdan sonra değişmiş. Bu bir hata değil,
                // bilinçli bir koruma: kullanıcının yaptığı değişiklik
                // uygulanmadan üstüne yazılmasın. Mesaj, denetim alanlarından
                // hareketle "kaydı kim değiştirdi" bilgisini taşır.
                return await HandleConcurrencyAsync(ex, cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                string innerMessage = ex.InnerException?.Message ?? ex.Message;

                TResponse? failure = TryFailure($"Veritabanı kayıt hatası: {innerMessage}");

                if (failure is not null)
                {
                    return failure;
                }

                throw;
            }

            return response;
        }

        /// <summary>
        /// Çakışma mesajını üretir ve işteki kayıt durumuna göre döndürür.
        /// </summary>
        /// <remarks>
        /// Yanıt bir <see cref="Result{T}"/> ise başarısızlık olarak döner;
        /// değilse (void dönen istekler) istisna fırlatılır, çünkü çağıran
        /// hatayı göremez.
        /// </remarks>
        private async Task<TResponse> HandleConcurrencyAsync(
            DbUpdateConcurrencyException exception,
            CancellationToken cancellationToken)
        {
            string message;

            try
            {
                message = await _conflictResolver.BuildMessageAsync(exception, cancellationToken);
            }
            catch (Exception resolverFailure)
            {
                // Mesaj üretimi başarısız olursa çakışmanın kendisi kaybolmaz;
                // yalnızca ayrıntı kaybı olur.
                System.Diagnostics.Debug.WriteLine(
                    $"[TransactionBehavior] Çakışma mesajı üretilemedi: {resolverFailure.Message}");

                message = "Kayıt sizin yaptığınız değişiklikten sonra başka biri tarafından "
                          + "değiştirildi. Yaptığınız değişiklikler kaydedilmedi.";
            }

            return TryFailure(message)
                   ?? throw new ConcurrencyConflictException(message);
        }

        /// <summary>
        /// <typeparamref name="TResponse"/> bir <see cref="Result{T}"/> ise
        /// başarısızlık sonucu üretir; değilse <see langword="null"/> döner.
        /// </summary>
        private static TResponse? TryFailure(string message)
        {
            if (!typeof(TResponse).IsGenericType
                || typeof(TResponse).GetGenericTypeDefinition() != typeof(Result<>))
            {
                return default;
            }

            MethodInfo? failureMethod = typeof(Result<>)
                .MakeGenericType(typeof(TResponse).GetGenericArguments()[0])
                .GetMethod("Failure", [typeof(string)]);

            return failureMethod is null
                ? default
                : (TResponse)failureMethod.Invoke(null, [message])!;
        }
    }
}