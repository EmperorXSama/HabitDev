using HabitDev.Database;
using Microsoft.EntityFrameworkCore;

namespace HabitDev.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        await using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await using ApplicationIdentityDbContext identityDbContext = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();

        try
        {
            await dbContext.Database.MigrateAsync();
            app.Logger.LogInformation("application migration successfully applied.");
            await identityDbContext.Database.MigrateAsync();
            app.Logger.LogInformation("identity migration successfully applied.");
        }
        catch (Exception e)
        {
            app.Logger.LogError(e,"Error occured while applying migrations");
            throw;
        }
    }
}
