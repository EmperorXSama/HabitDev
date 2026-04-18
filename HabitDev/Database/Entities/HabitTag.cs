namespace HabitDev.Database.Entities;

public sealed class HabitTag
{
    public string HabitId { get; set; }
    public string TagId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    
    
    public Habit Habit { get; set; }
    public Tag Tag { get; set; }
}
