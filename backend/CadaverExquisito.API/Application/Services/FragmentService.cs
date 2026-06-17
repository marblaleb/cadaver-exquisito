using CadaverExquisito.API.Application.DTOs;
using CadaverExquisito.API.Application.Interfaces;
using CadaverExquisito.API.Domain.Entities;

namespace CadaverExquisito.API.Application.Services;

public class FragmentService(
    ICadaverRepository cadaverRepo,
    IFragmentRepository fragmentRepo,
    INotificationService notifications)
{
    public async Task<FragmentDto?> GetLastFragmentAsync(Guid cadaverId)
    {
        var cadaver = await cadaverRepo.GetByIdAsync(cadaverId)
            ?? throw new KeyNotFoundException($"Cadaver {cadaverId} not found");

        if (cadaver.CurrentTurn == 1) return null;

        var fragment = await fragmentRepo.GetBySequenceOrderAsync(cadaverId, cadaver.CurrentTurn - 1);
        return fragment is null ? null : new FragmentDto(fragment.Id, fragment.Content, fragment.SequenceOrder);
    }

    public async Task<FragmentDto> AddFragmentAsync(Guid cadaverId, string userId, AddFragmentRequest request)
    {
        var cadaver = await cadaverRepo.GetByIdAsync(cadaverId)
            ?? throw new KeyNotFoundException($"Cadaver {cadaverId} not found");

        if (await fragmentRepo.UserHasFragmentAsync(cadaverId, userId))
            throw new InvalidOperationException("User has already participated in this cadaver.");

        if (request.Content.Split(' ').Length > 300)
            throw new ArgumentException("Content must not exceed 300 words.");

        var fragment = new Fragment
        {
            CadaverId = cadaverId,
            UserId = userId,
            Content = request.Content,
            SequenceOrder = cadaver.CurrentTurn,
            CreatedAt = DateTime.UtcNow
        };

        var saved = await fragmentRepo.CreateAsync(fragment);

        cadaver.CurrentTurn++;
        if (cadaver.CurrentTurn > cadaver.MaxParticipants)
        {
            cadaver.IsCompleted = true;
            await cadaverRepo.UpdateAsync(cadaver);
            _ = NotifyCompletedAsync(cadaverId);
        }
        else
        {
            await cadaverRepo.UpdateAsync(cadaver);
            _ = NotifyNextParticipantsAsync(cadaverId);
        }

        return new FragmentDto(saved.Id, saved.Content, saved.SequenceOrder);
    }

    public async Task<FullCadaverDto> GetFullCadaverAsync(Guid cadaverId)
    {
        var cadaver = await cadaverRepo.GetByIdAsync(cadaverId)
            ?? throw new KeyNotFoundException($"Cadaver {cadaverId} not found");

        if (!cadaver.IsCompleted)
            throw new InvalidOperationException("Cadaver is not completed yet.");

        var fragments = await fragmentRepo.GetAllByCadaverOrderedAsync(cadaverId);
        var fullFragments = fragments.Select(f =>
            new FullFragmentDto(f.Content, f.SequenceOrder, f.User?.Name ?? "Anónimo")).ToList();

        return new FullCadaverDto(cadaver.Id, cadaver.Title, fullFragments);
    }

    private async Task NotifyNextParticipantsAsync(Guid cadaverId)
    {
        try
        {
            await notifications.SendToNonParticipantsAsync(cadaverId, "Cadáver Exquisito", "Hay una historia esperando tu continuación.");
        }
        catch { }
    }

    private async Task NotifyCompletedAsync(Guid cadaverId)
    {
        try
        {
            var participantIds = await fragmentRepo.GetParticipantUserIdsAsync(cadaverId);
            await notifications.SendToUsersAsync(participantIds, "Cadáver Exquisito", "La historia en la que participaste está completa.");
        }
        catch { }
    }
}
