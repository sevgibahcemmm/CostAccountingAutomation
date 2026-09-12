using Cost.Accounting.Automation.Application.Services;
using Cost.Accounting.Automation.Domain.Roles;
using Cost.Accounting.Automation.Domain.Users;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TS.MediatR;

namespace Cost.Accounting.Automation.Application.Behaviors;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class PermissionAttribute : Attribute
{
    public string? Permission { get; }
    public PermissionAttribute() { }
    public PermissionAttribute(string permission) => Permission = permission;
}

public sealed class PermissionBehavior<TRequest, TResponse>(
    IServiceScopeFactory _scopeFactory,
    IClaimContext _userContext)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        var attr = request.GetType().GetCustomAttribute<PermissionAttribute>(inherit: true);
        if (attr is null) return await next();

        var userId = _userContext.GetUserId();

        if (userId == Guid.Empty) return await next();

        await using var scope = _scopeFactory.CreateAsyncScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();

        var user = await userRepository.FirstOrDefaultAsync(p => p.Id == userId, cancellationToken);
        if (user is null) throw new AuthorizationException("Kullanıcı bulunamadı.");

        var role = await roleRepository.FirstOrDefaultAsync(r => r.Id == user.RoleId, cancellationToken);
        if (role is null) throw new AuthorizationException("Kullanıcıya atanmış geçerli bir rol bulunamadı.");

        if (role.Name.Value == "sys_admin") return await next();

        if (!string.IsNullOrEmpty(attr.Permission))
        {
            bool hasPermission = role.Permissions.Any(p => p.Value == attr.Permission);
            if (!hasPermission)
                throw new AuthorizationException($"'{attr.Permission}' yetkisine sahip değilsiniz.");
        }
        else if (role.Name.Value != "Admin")
        {
            throw new AuthorizationException("Bu işlem için admin yetkisi gereklidir.");
        }

        return await next();
    }
}

public sealed class AuthorizationException : Exception
{
    public AuthorizationException() : base("Yetkiniz bulunmamaktadır.") { }
    public AuthorizationException(string message) : base(message) { }
}