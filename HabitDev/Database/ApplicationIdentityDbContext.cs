using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HabitDev.Database;

public class ApplicationIdentityDbContext(DbContextOptions<ApplicationIdentityDbContext> options)
:IdentityDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema(Schemas.Identity);
    }
}
