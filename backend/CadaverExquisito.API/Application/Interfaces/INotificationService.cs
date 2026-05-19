namespace CadaverExquisito.API.Application.Interfaces;

public interface INotificationService
{
    Task SendToUsersAsync(List<string> userIds, string title, string body);
    Task SendToNonParticipantsAsync(Guid cadaverId, string title, string body);
}
