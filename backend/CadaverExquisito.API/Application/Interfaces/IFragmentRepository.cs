using CadaverExquisito.API.Domain.Entities;

namespace CadaverExquisito.API.Application.Interfaces;

public interface IFragmentRepository
{
    Task<Fragment?> GetBySequenceOrderAsync(Guid cadaverId, int sequenceOrder);
    Task<bool> UserHasFragmentAsync(Guid cadaverId, string userId);
    Task<Fragment> CreateAsync(Fragment fragment);
    Task<List<Fragment>> GetAllByCadaverOrderedAsync(Guid cadaverId);
    Task<List<string>> GetParticipantUserIdsAsync(Guid cadaverId);
}
