using CadaverExquisito.API.Domain.Entities;

namespace CadaverExquisito.API.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task UpsertAsync(User user);
    Task UpdateFcmTokenAsync(string userId, string fcmToken);
    Task<List<string>> GetFcmTokensExcludingParticipantsAsync(Guid cadaverId);
}
