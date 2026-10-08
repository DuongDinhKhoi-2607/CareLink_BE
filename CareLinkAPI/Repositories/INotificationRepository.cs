using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface INotificationRepository
{
    /// <summary>Stages a notification; it is persisted with the next <see cref="IUnitOfWork.SaveChangesAsync"/>.</summary>
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
}
