using Cost.Accounting.Automation.Application.Behaviors;
using Cost.Accounting.Automation.Domain.Abstractions;
using Cost.Accounting.Automation.Domain.Products;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Products;

[Permission("product:view")]
public sealed record ProductGetAllQuery(
    bool OnlyDeleted = false) : IRequest<IQueryable<ProductDto>>
{
    public ProductGetAllQuery() : this(false) { }
}

internal sealed class ProductGetAllQueryHandler(
    IProductRepository productRepository) : IRequestHandler<ProductGetAllQuery, IQueryable<ProductDto>>
{
    public Task<IQueryable<ProductDto>> Handle(ProductGetAllQuery request, CancellationToken cancellationToken)
    {
        IQueryable<EntityWithAuditDto<Product>> source = request.OnlyDeleted
            ? productRepository.GetAllWithAuditIncludingDeleted().Where(i => i.Entity.IsDeleted)
            : productRepository.GetAllWithAudit();

        return Task.FromResult(source.MapTo().AsQueryable());
    }
}