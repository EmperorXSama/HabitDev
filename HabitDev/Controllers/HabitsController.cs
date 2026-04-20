using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using FluentValidation;
using HabitDev.Database;
using HabitDev.Database.Entities;
using HabitDev.DTOs;
using HabitDev.DTOs.Habits;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using HabitDev.Services.Sorting;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace HabitDev.Controllers;


[ApiController]
[Route("Habits")]
public sealed  class HabitsController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HabitDto>> GetHabits(
        [FromQuery] HabitQueryParameter queryParameter,
        [FromServices] SortMappingProvider sortMappingProvider)
    {
        if (!sortMappingProvider.ValidateMappings<HabitDto,Habit>(queryParameter.Sort))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"the provided sort parameter isn't valid : ' {queryParameter.Sort}'"
            );
        }
        queryParameter.Search ??= queryParameter.Search?.Trim().ToLower();

        SortMapping[] sortMappings = sortMappingProvider.GetMappings<HabitDto, Habit>();
        List<HabitDto> habits = await context.Habits
            .Where(h => queryParameter.Search == null ||
                        h.Name.ToLower().Contains(queryParameter.Search)
                        || h.Description != null && h.Description.ToLower().Contains(queryParameter.Search))
            .Where(h => queryParameter.Status == null  || h.Status ==  queryParameter.Status)
            .ApplySort(queryParameter.Sort,sortMappings)
            .Where(h =>  queryParameter.Type == null ||   queryParameter.Type == h.Type)
            .Select(HabitQueries.ProjectToDto())
            .ToListAsync();

        var habitCollection = new HabitsCollectionDto()
        {
            Data = habits
        };
        return Ok(habitCollection);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DetailedHabit>> GetHabit(string id)
    {
        DetailedHabit habit = await context.
            Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToDetailedDto())
            .FirstOrDefaultAsync();

        return habit == null ? NotFound() : Ok(habit);
    }
    
    [HttpPost]
    public async Task<ActionResult<HabitDto>> CreateHabit( CreateHabitDto habitDto,
        [FromServices] IValidator<CreateHabitDto> validator)
    {
        await validator.ValidateAndThrowAsync(habitDto);
       
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


