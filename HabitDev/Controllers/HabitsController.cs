using HabitDev.Database;
using HabitDev.Database.Entities;
using HabitDev.DTOs;
using HabitDev.DTOs.Habits;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitDev.Controllers;


[ApiController]
[Route("Habits")]
public sealed  class HabitsController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HabitDto>> GetHabits()
    {
        List<HabitDto> habits = await context.
            Habits
            .Select(HabitQueries.ProjectToDto())
            .ToListAsync();

        var habitCollection = new HabitsCollectionDto()
        {
            Data = habits
        };
        return Ok(habitCollection);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HabitDto>> GetHabit(string id)
    {
        HabitDto habit = await context.
            Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToDto())
            .FirstOrDefaultAsync();

        return habit == null ? NotFound() : Ok(habit);
    }
    
    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit( CreateHabitDto habitDto)
    {
        Habit habit = habitDto.ToEntity();
        context.Habits.Add(habit);

        await context.SaveChangesAsync();
        HabitDto habitdto = habit.ToDto();
        return CreatedAtAction(nameof(GetHabit), new { id = habitdto.Id }, habitDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit(string id, UpdateHabitDto updateHabitDto)
    {
        Habit? habit = await context.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null)
        {
            return NotFound();
        }

        habit.UpdateFromDto(updateHabitDto);

        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult> PatchHabit(string id, JsonPatchDocument<HabitDto> patchDocument)
    {
          
        Habit? habit = await context.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null)
        {
            return NotFound();
        }

        HabitDto habitDto = habit.ToDto();
        patchDocument.ApplyTo(habitDto,ModelState);
        if (!TryValidateModel(habitDto))
        {
            return ValidationProblem(ModelState);
        }

        habit.Name = habitDto.Name;
        habit.Description = habitDto.Description;
        habit.UpdatedAtUtc = habitDto.UpdatedAtUtc;

        await context.SaveChangesAsync();

        return NoContent();
    }


    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHabit(string id)
    {
        Habit? habit = await context.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null)
        {
            return NotFound();
        }

        context.Habits.Remove(habit);
        return NoContent();
    }
}


