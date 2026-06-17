using CadaverExquisito.API.Application.DTOs;
using CadaverExquisito.API.Application.Interfaces;
using CadaverExquisito.API.Domain.Entities;

namespace CadaverExquisito.API.Application.Services;

public class CadaverService(ICadaverRepository cadaverRepo, IFragmentRepository fragmentRepo)
{
    public async Task<List<CadaverDto>> GetAvailableAsync(string userId)
    {
        var cadavers = await cadaverRepo.GetAvailableForUserAsync(userId);
        return cadavers.Select(ToDto).ToList();
    }

    public async Task<List<CadaverDto>> GetPendingAsync(string userId)
    {
        var cadavers = await cadaverRepo.GetPendingForUserAsync(userId);
        return cadavers.Select(ToDto).ToList();
    }

    public async Task<List<CadaverDto>> GetCompletedAsync()
    {
        var cadavers = await cadaverRepo.GetCompletedAsync();
        return cadavers.Select(ToDto).ToList();
    }

    public async Task<CadaverDto> CreateAsync(string userId, CreateCadaverRequest request)
    {
        var now = DateTime.UtcNow;
        var cadaver = new Cadaver
        {
            Title = $"Historia del {now:dd MMM yyyy}",
            MaxParticipants = request.MaxParticipants,
            CurrentTurn = 1,
            CreatedAt = now,
            CreatedByUserId = userId
        };

        var saved = await cadaverRepo.CreateAsync(cadaver);

        var fragment = new Fragment
        {
            CadaverId = saved.Id,
            UserId = userId,
            Content = request.Content,
            SequenceOrder = 1,
            CreatedAt = now
        };
        await fragmentRepo.CreateAsync(fragment);

        saved.CurrentTurn = 2;
        if (saved.CurrentTurn > saved.MaxParticipants)
            saved.IsCompleted = true;

        await cadaverRepo.UpdateAsync(saved);
        return ToDto(saved);
    }

    private static CadaverDto ToDto(Cadaver c) =>
        new(c.Id, c.Title, c.MaxParticipants, c.CurrentTurn, c.CreatedAt);
}
