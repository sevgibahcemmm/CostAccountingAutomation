
//using GenericRepository;
//using TS.MediatR;

//namespace Cost.Accounting.Automation.Application.Behaviors
//{
//    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
//        where TRequest : IRequest<TResponse>
//    {
//        private readonly IUnitOfWork _unitOfWork;

//        public TransactionBehavior(IUnitOfWork unitOfWork)
//        {
//            _unitOfWork = unitOfWork;
//        }

//        public async Task<TResponse> Handle(
//            TRequest request,
//            RequestHandlerDelegate<TResponse> next,
//            CancellationToken cancellationToken)
//        {
//            var response = await next();
//            await _unitOfWork.SaveChangesAsync(cancellationToken);
//            return response;
//        }
//    }
//}


using Cost.Accounting.Automation.Application.Services;
using GenericRepository;
using TS.MediatR;
using TS.Result; // Result yapınızın namespace'i

namespace Cost.Accounting.Automation.Application.Behaviors
{
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMasterUnitOfWork _masterUnitOfWork;

        public TransactionBehavior(IUnitOfWork unitOfWork, IMasterUnitOfWork masterUnitOfWork)
        {
            _unitOfWork = unitOfWork;
            _masterUnitOfWork = masterUnitOfWork;
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
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                
                // Eğer TResponse bir Result türündeyse hata dönebiliriz:
                if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var failureMethod = typeof(Result<>).MakeGenericType(typeof(TResponse).GetGenericArguments()[0])
                        .GetMethod("Failure", new[] { typeof(string) });
                    
                    if (failureMethod != null)
                    {
                        return (TResponse)failureMethod.Invoke(null, new object[] { $"Veritabanı kayıt hatası: {innerMessage}" })!;
                    }
                }
                
                throw;
            }

            return response;
        }
    }
}