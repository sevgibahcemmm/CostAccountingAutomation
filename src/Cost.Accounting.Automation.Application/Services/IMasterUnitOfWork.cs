namespace Cost.Accounting.Automation.Application.Services;

/// <summary>
/// Master (merkezi) veritabanı için birim işi. <c>IUnitOfWork</c> yıl
/// veritabanına bağlıdır; şirket, kullanıcı, rol ve mali yıl kayıtları
/// master'da tutulduğu için bu kayıtlarla çalışan servisler bu arayüzü
/// kullanmalıdır. Aksi halde değişiklikler yanlış veritabanına yazılır.
/// </summary>
public interface IMasterUnitOfWork
{
    bool HasChanges();

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
