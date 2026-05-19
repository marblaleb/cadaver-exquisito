using CadaverExquisito.API.Domain.Entities;

namespace CadaverExquisito.API.Application.Interfaces;

public interface ICadaverRepository
{
    Task<List<Cadaver>> GetAvailableForUserAsync(string userId);
    Task<List<Cadaver>> GetPendingForUserAsync(string userId);
    Task<List<Cadaver>> GetCompletedAsync();
    Task<Cadaver?> GetByIdAsync(Guid id);
    Task<Cadaver> CreateAsync(Cadaver cadaver);
    Task UpdateAsync(Cadaver cadaver);
}
