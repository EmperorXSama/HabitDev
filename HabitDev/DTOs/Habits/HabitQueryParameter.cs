using HabitDev.Database.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HabitDev.DTOs.Habits;

public class HabitQueryParameter
{
    [FromQuery(Name = "q")]
    public string? Search { get; set; }
    public HabitStatus? Status { get; init; }
    public HabitType? Type { get; init; }
    
    public string? Sort { get; init; }
}
