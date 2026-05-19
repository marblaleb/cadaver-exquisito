using CadaverExquisito.API.Application.Interfaces;
using CadaverExquisito.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CadaverExquisito.API.Infrastructure.Data.Repositories;

public class FragmentRepository(AppDbContext db) : IFragmentRepository
{
    public async Task<Fragment?> GetBySequenceOrderAsync(Guid cadaverId, int sequenceOrder) =>
        await db.Fragments
            .FirstOrDefaultAsync(f => f.CadaverId == cadaverId && f.SequenceOrder == sequenceOrder);

    public async Task<bool> UserHasFragmentAsync(Guid cadaverId, string userId) =>
        await db.Fragments.AnyAsync(f => f.CadaverId == cadaverId && f.UserId == userId);

    public async Task<Fragment> CreateAsync(Fragment fragment)
    {
        db.Fragments.Add(fragment);
        await db.SaveChangesAsync();
        return fragment;
    }

    public async Task<List<Fragment>> GetAllByCadaverOrderedAsync(Guid cadaverId) =>
        await db.Fragments
            .Where(f => f.CadaverId == cadaverId)
            .Include(f => f.User)
            .OrderBy(f => f.SequenceOrder)
            .ToListAsync();

    public async Task<List<string>> GetParticipantUserIdsAsync(Guid cadaverId) =>
        await db.Fragments
            .Where(f => f.CadaverId == cadaverId)
            .Select(f => f.UserId)
            .Distinct()
            .ToListAsync();
}
