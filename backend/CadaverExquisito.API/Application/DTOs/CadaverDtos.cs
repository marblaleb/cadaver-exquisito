namespace CadaverExquisito.API.Application.DTOs;

public record CreateCadaverRequest(int MaxParticipants, string Content);

public record CadaverDto(
    Guid Id,
    string Title,
    int MaxParticipants,
    int CurrentTurn,
    DateTime CreatedAt);

public record FullCadaverDto(
    Guid Id,
    string Title,
    List<FullFragmentDto> Fragments);

public record FullFragmentDto(
    string Content,
    int SequenceOrder,
    string AuthorName);
