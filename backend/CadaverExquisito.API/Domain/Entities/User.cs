namespace CadaverExquisito.API.Domain.Entities;

public class User
{
    public string Id { get; set; } = string.Empty; // Firebase UID
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FcmToken { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Cadaver> CreatedCadavers { get; set; } = new List<Cadaver>();
    public ICollection<Fragment> Fragments { get; set; } = new List<Fragment>();
}
