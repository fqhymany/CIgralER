using LawyerProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawyerProject.Infrastructure.Data.Configurations;

public class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Created)
            .HasDefaultValueSql("(getdate())")
            .HasColumnType("datetime");
        builder.Property(e => e.LastModified)
            .HasDefaultValueSql("(getdate())")
            .HasColumnType("datetime");

        builder.HasOne(cp => cp.Region).WithMany(c => c.UserPreferences)
            .HasForeignKey(cp => cp.RegionId);
        builder.HasOne(d => d.User).WithMany(p => p.UserPreferences)
            .HasForeignKey(d => d.UserId);
        builder.HasOne(d => d.PreferenceKey).WithMany(p => p.UserPreferences)
            .HasForeignKey(d => d.Key);

    }
}
