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
using HabitDev.DTOs.Common;
using HabitDev.Helpers;
using HabitDev.Migrations;
using HabitDev.Services;
using HabitDev.Services.Sorting;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace HabitDev.Controllers;


[ApiController]
[Route("Habits")]
public sealed  class HabitsController(ApplicationDbContext context , GenerateLinksService generateLinksService) : ControllerBase
{
    public static readonly  string Name = nameof(HabitsController).Replace("Controller", string.Empty);
    
    [HttpGet]
    public async Task<ActionResult<PaginationResponse<HabitDto>>> GetHabits(
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
        IQueryable<HabitDto> habitsQuery = context.Habits
            .Where(h => queryParameter.Search == null ||
                        h.Name.ToLower().Contains(queryParameter.Search)
                        || h.Description != null && h.Description.ToLower().Contains(queryParameter.Search))
            .Where(h => queryParameter.Status == null || h.Status == queryParameter.Status)
            .Where(h => queryParameter.Type == null || queryParameter.Type == h.Type)
            .ApplySort(queryParameter.Sort, sortMappings)
            .Select(HabitQueries.ProjectToDto());


        bool includeLinks = queryParameter.Accept == CustomMediaTypeNames.Application.HateoasJson;
        if (includeLinks)
        {
            var pagedResult = 
                await PaginationResponse<HabitDto>.CreateAsync(
                    habitsQuery,
                    queryParameter.Page,
                    queryParameter.PageSize,
                    generateLinksService.WrapHabitWithLinks
                );

            pagedResult.Links = generateLinksService.CreateLinksForHabitCollection(
                queryParameter,
                pagedResult.HasPreviousPage,
                pagedResult.HasNextPage);

            return Ok(pagedResult);
        }
        else
        {
            var pagedResult = 
                await PaginationResponse<HabitDto>.CreateAsync(
                    habitsQuery,
                    queryParameter.Page,
                    queryParameter.PageSize
                );

            return Ok(pagedResult);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HateoasResponse<DetailedHabit>>> GetHabit(string id , [FromHeader(Name = "Accept")] string? accept)
    {
        DetailedHabit habit = await context.
            Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToDetailedDto())
            .FirstOrDefaultAsync();
        if (habit == null)
        {
            return NotFound();
        }
        var response = new HateoasResponse<DetailedHabit>
        {
            Data = habit,
            Links = accept == CustomMediaTypeNames.Application.HateoasJson ? generateLinksService.CreateLinksForHabit(id) : []
        };
        return Ok(response);
       
    }
    
    [HttpPost]
    public async Task<ActionResult<HateoasResponse<HabitDto>>> CreateHabit( CreateHabitDto habitDto,
        [FromServices] IValidator<CreateHabitDto> validator)
    {
        await validator.ValidateAndThrowAsync(habitDto);
       
        Habit habit = habitDto.ToEntity();
        context.Habits.Add(habit);

        await context.SaveChangesAsync();
        HabitDto habitdto = habit.ToDto();
        var response = new HateoasResponse<HabitDto>
        {
            Data = habitdto,
            Links =generateLinksService.CreateLinksForHabit(habitdto.Id)
        };
        return CreatedAtAction(nameof(GetHabit), new { id = habitdto.Id }, response);
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


