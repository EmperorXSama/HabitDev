using HabitDev.Database;
using Microsoft.EntityFrameworkCore;

namespace HabitDev.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        await using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            await dbContext.Database.MigrateAsync();
            app.Logger.LogInformation("database migration successfully applied.");
        }
        catch (Exception e)
        {
            app.Logger.LogError(e,"Error occured while applying migrations");
            throw;
        }
    }
}
