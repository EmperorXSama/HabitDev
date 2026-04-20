using HabitDev.DTOs.Common;

namespace HabitDev.DTOs.Tags;

public sealed record TagsCollectionDto : ICollectionResponse<TagDto>
{
    public List<TagDto> Items { get; set; }
}
