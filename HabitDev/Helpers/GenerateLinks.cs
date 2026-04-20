using HabitDev.Controllers;
using HabitDev.Database.Entities;
using HabitDev.DTOs.Common;
using HabitDev.DTOs.Habits;
using HabitDev.Migrations;
using HabitDev.Services;

namespace HabitDev.Helpers;

public class GenerateLinksService(LinkService linkService)
{
    public HateoasResponse<HabitDto> WrapHabitWithLinks(HabitDto habit)
    {
        return new HateoasResponse<HabitDto>
        {
            Data = habit,
            Links = CreateLinksForHabit(habit.Id)
        };
    }
    
    public List<LinkDto> CreateLinksForHabit(string id)
    {
        List<LinkDto> links =
        [
            linkService.Create(nameof(HabitsController.GetHabit), "self", HttpMethods.Get, new { id },HabitsController.Name),
            linkService.Create(nameof(HabitsController.UpdateHabit), "update", HttpMethods.Put, new { id },HabitsController.Name),
            linkService.Create(nameof(HabitsController.PatchHabit), "partial-update", HttpMethods.Patch, new { id },HabitsController.Name),
            linkService.Create(nameof(HabitsController.DeleteHabit), "delete", HttpMethods.Delete, new { id },HabitsController.Name),
            linkService.Create(nameof(HabitTagsController.UpsertHabitTags), "upsert-tag", HttpMethods.Put, new { habitId = id },HabitTagsController.Name),
        ];

        return links;
    }

    public List<LinkDto> CreateLinksForHabitCollection(HabitQueryParameter queryParameter , bool hasPrevious , bool hasNext)
    {
        List<LinkDto> links =
        [
            linkService.Create(nameof(HabitsController.GetHabits),
                "self",
                HttpMethods.Get,
                new
                {
                    page = queryParameter.Page,
                    pageSize= queryParameter.PageSize,
                    q = queryParameter.Search,
                    sort = queryParameter.Sort,
                    type = queryParameter.Type,
                    status = queryParameter.Status
                }
                ),
            
        ];

        if (hasPrevious)
        {
            links.Add(
                linkService.Create(nameof(HabitsController.GetHabits), "prev-page",
                    HttpMethods.Get,
                    new
                    {
                        page = queryParameter.Page - 1,
                        pageSize= queryParameter.PageSize,
                        q = queryParameter.Search,
                        sort = queryParameter.Sort,
                        type = queryParameter.Type,
                        status = queryParameter.Status,
                        
                    })
                );
        }
        if (hasNext)
        {
            links.Add(
                linkService.Create(nameof(HabitsController.GetHabits), "next-page",
                    HttpMethods.Get,
                    new
                    {
                        page = queryParameter.Page + 1,
                        pageSize= queryParameter.PageSize,
                        q = queryParameter.Search,
                        sort = queryParameter.Sort,
                        type = queryParameter.Type,
                        status = queryParameter.Status,
                        
                    })
            );
        }
        return links;
    }
}
