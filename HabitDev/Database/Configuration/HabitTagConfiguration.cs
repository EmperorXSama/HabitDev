using HabitDev.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HabitDev.Database.Configuration;

public class HabitTagConfiguration : IEntityTypeConfiguration<HabitTag>
{
    public void Configure(EntityTypeBuilder<HabitTag> builder)
    {
        builder.HasKey(ht => new { ht.HabitId, ht.TagId });
        builder.Property(h => h.HabitId).HasMaxLength(500);
        builder.Property(h => h.TagId).HasMaxLength(500);
        builder.HasOne(ht => ht.Habit)
            .WithMany(h => h.HabitTags)
            .HasForeignKey(ht => ht.HabitId);

        builder.HasOne(ht => ht.Tag)
            .WithMany(t => t.HabitTags)
            .HasForeignKey(ht => ht.TagId);
    }
}
