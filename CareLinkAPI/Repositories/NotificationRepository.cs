using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public class NotificationRepository(CareLinkDbContext db) : INotificationRepository
{
    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default) =>
        await db.Notifications.AddAsync(notification, cancellationToken);
}
