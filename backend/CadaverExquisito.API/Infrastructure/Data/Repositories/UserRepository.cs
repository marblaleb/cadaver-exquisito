using CadaverExquisito.API.Application.Interfaces;
using CadaverExquisito.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CadaverExquisito.API.Infrastructure.Data.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<User?> GetByIdAsync(string id) =>
        await db.Users.FindAsync(id);

    public async Task UpsertAsync(User user)
    {
        var existing = await db.Users.FindAsync(user.Id);
        if (existing is null)
            db.Users.Add(user);
        else
        {
            existing.Name = user.Name;
            existing.Email = user.Email;
            if (user.FcmToken is not null) existing.FcmToken = user.FcmToken;
        }
        await db.SaveChangesAsync();
    }

    public async Task UpdateFcmTokenAsync(string userId, string fcmToken)
    {
        var user = await db.Users.FindAsync(userId);
        if (user is not null)
        {
            user.FcmToken = fcmToken;
            await db.SaveChangesAsync();
        }
    }

    public async Task<List<string>> GetFcmTokensExcludingParticipantsAsync(Guid cadaverId)
    {
        var participantIds = await db.Fragments
            .Where(f => f.CadaverId == cadaverId)
            .Select(f => f.UserId)
            .ToListAsync();

        return await db.Users
            .Where(u => !participantIds.Contains(u.Id) && u.FcmToken != null)
            .Select(u => u.FcmToken!)
            .ToListAsync();
    }
}
