namespace CadaverExquisito.API.Application.DTOs;

public record RegisterUserRequest(string Name, string Email, string? FcmToken);
public record UpdateFcmTokenRequest(string FcmToken);
