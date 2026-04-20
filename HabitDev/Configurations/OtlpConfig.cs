using System.ComponentModel.DataAnnotations;

namespace HabitDev.Configurations;

public class OtlpConfig
{
    public const string SectionName = "Otel";

    [Required]
    [Url]
    public string Endpoint { get; set; } = default!;
}
