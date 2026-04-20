namespace HabitDev.Database.Entities;

public sealed class Tag
{
    public string Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    
    public ICollection<HabitTag> HabitTags { get; set; } = [];
}
