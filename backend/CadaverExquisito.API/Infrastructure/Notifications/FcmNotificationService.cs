using CadaverExquisito.API.Application.Interfaces;
using FirebaseAdmin.Messaging;

namespace CadaverExquisito.API.Infrastructure.Notifications;

public class FcmNotificationService(IUserRepository userRepo) : INotificationService
{
    public async Task SendToUsersAsync(List<string> userIds, string title, string body)
    {
        if (userIds.Count == 0) return;

        var tokens = new List<string>();
        foreach (var id in userIds)
        {
            var user = await userRepo.GetByIdAsync(id);
            if (user?.FcmToken is not null)
                tokens.Add(user.FcmToken);
        }

        if (tokens.Count == 0) return;

        var message = new MulticastMessage
        {
            Tokens = tokens,
            Notification = new Notification { Title = title, Body = body }
        };

        await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
    }

    public async Task SendToNonParticipantsAsync(Guid cadaverId, string title, string body)
    {
        var tokens = await userRepo.GetFcmTokensExcludingParticipantsAsync(cadaverId);
        if (tokens.Count == 0) return;

        var message = new MulticastMessage
        {
            Tokens = tokens,
            Notification = new Notification { Title = title, Body = body }
        };

        await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
    }
}
