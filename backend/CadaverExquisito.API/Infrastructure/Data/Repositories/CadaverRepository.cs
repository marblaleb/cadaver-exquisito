using CadaverExquisito.API.Application.Interfaces;
using CadaverExquisito.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CadaverExquisito.API.Infrastructure.Data.Repositories;

public class CadaverRepository(AppDbContext db) : ICadaverRepository
{
    public async Task<List<Cadaver>> GetAvailableForUserAsync(string userId)
    {
        var participatedIds = await db.Fragments
            .Where(f => f.UserId == userId)
            .Select(f => f.CadaverId)
            .ToListAsync();

        return await db.Cadavers
            .Where(c => !c.IsCompleted && !participatedIds.Contains(c.Id))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Cadaver>> GetPendingForUserAsync(string userId)
    {
        var participatedIds = await db.Fragments
            .Where(f => f.UserId == userId)
            .Select(f => f.CadaverId)
            .ToListAsync();

        return await db.Cadavers
            .Where(c => !c.IsCompleted && participatedIds.Contains(c.Id))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Cadaver>> GetCompletedAsync() =>
        await db.Cadavers
            .Where(c => c.IsCompleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    public async Task<Cadaver?> GetByIdAsync(Guid id) =>
        await db.Cadavers.FindAsync(id);

    public async Task<Cadaver> CreateAsync(Cadaver cadaver)
    {
        db.Cadavers.Add(cadaver);
        await db.SaveChangesAsync();
        return cadaver;
    }

    public async Task UpdateAsync(Cadaver cadaver)
    {
        db.Cadavers.Update(cadaver);
        await db.SaveChangesAsync();
    }
}
