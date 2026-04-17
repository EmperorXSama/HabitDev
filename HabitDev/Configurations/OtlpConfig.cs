using System.ComponentModel.DataAnnotations;

namespace HabitDev.Configurations;

public class OtlpConfig
{
    public const string SectionName = "Otlp";

    [Required]
    [Url]
    public string Endpoint { get; set; } = default!;
}
