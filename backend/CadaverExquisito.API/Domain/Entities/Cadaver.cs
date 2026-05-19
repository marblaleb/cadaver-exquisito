namespace CadaverExquisito.API.Domain.Entities;

public class Cadaver
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public int MaxParticipants { get; set; }
    public int CurrentTurn { get; set; } = 1;
    public bool IsCompleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string CreatedByUserId { get; set; } = string.Empty;
    public User CreatedByUser { get; set; } = null!;
    public ICollection<Fragment> Fragments { get; set; } = new List<Fragment>();
}
