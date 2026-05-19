namespace CadaverExquisito.API.Domain.Entities;

public class Fragment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CadaverId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int SequenceOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Cadaver Cadaver { get; set; } = null!;
    public User User { get; set; } = null!;
}
