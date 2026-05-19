namespace CadaverExquisito.API.Application.DTOs;

public record AddFragmentRequest(string Content);

public record FragmentDto(
    Guid Id,
    string Content,
    int SequenceOrder);
