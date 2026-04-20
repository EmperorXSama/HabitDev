using HabitDev.Database;
using HabitDev.Database.Entities;
using HabitDev.DTOs.HabitTags;
using HabitDev.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitDev.Controllers;

[ApiController]
[Route("habits/{habitId}/tags")]
public class HabitTagsController(ApplicationDbContext  dbContext) : ControllerBase
{
    public static readonly  string Name = nameof(HabitTagsController).Replace("Controller", string.Empty);
    // upsert list of tags 
    [HttpPut]
    public async Task<ActionResult> UpsertHabitTags(string habitId, UpsertHabitTagsDto upsertHabitTags)
    {
        Habit? habit = await dbContext.Habits
            .Include(h => h.HabitTags)
            .FirstOrDefaultAsync(h => h.Id == habitId);

        if (habit is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Habit was not found",
                Detail = $"A habit with id {habitId} was not found",
                Extensions = new Dictionary<string, object?>
                {
                    ["habit-id"] = habitId
                }
            });
        }

        var currentTags = habit.HabitTags.Select(ht => ht.TagId).ToHashSet();

        if (currentTags.SetEquals(upsertHabitTags.TagIds))
        {
            return NoContent();
        }

        List<string> existingTagIds = await dbContext
            .Tags.Where(t => upsertHabitTags.TagIds.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync();

        if (existingTagIds.Count != upsertHabitTags.TagIds.Count)
        {
            return BadRequest("one or more tagIds is  invalid");
        }

        habit.HabitTags.RemoveAll(ht => !upsertHabitTags.TagIds.Contains(ht.TagId));
        string[] tagsToAdd = upsertHabitTags.TagIds.Except(currentTags).ToArray();
        habit.HabitTags.AddRange(tagsToAdd.Select(t => new HabitTag
        {
            HabitId = habitId,
            TagId = t,
            CreatedAtUtc = DateTime.UtcNow
        }));
        
        await dbContext.SaveChangesAsync();
        
        return NoContent();
    }
    
    // delete a tag

    [HttpDelete("{tagId}")]
    public async Task<ActionResult> DeleteHabitTag(string habitId, string tagId)
    {
        HabitTag? habitTag = await dbContext.HabitTags
            .SingleOrDefaultAsync(ht => ht.HabitId == habitId && ht.TagId == tagId);

        if (habitTag is null)
        {
            return NotFound();
        }

        dbContext.HabitTags.Remove(habitTag);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}
