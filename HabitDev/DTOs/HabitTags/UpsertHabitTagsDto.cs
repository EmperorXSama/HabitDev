namespace HabitDev.DTOs.HabitTags;

public sealed record UpsertHabitTagsDto(
    List<string> TagIds
    );
