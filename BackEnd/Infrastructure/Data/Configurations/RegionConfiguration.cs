using LawyerProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawyerProject.Infrastructure.Data.Configurations;

public class RegionConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        builder.HasKey(e => e.Id).HasName("PK__Regions__3214EC072F5190E2");

        builder.Property(e => e.Address).HasMaxLength(500);
        builder.Property(e => e.Created)
            .HasDefaultValueSql("(getdate())")
            .HasColumnType("datetime");
        builder.Property(e => e.DomainUrl).HasMaxLength(255);
        builder.Property(e => e.Email).HasMaxLength(100);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
        builder.Property(e => e.PhoneNumber).HasMaxLength(20);
        builder.Property(e => e.LastModified)
            .HasDefaultValueSql("(getdate())")
            .HasColumnType("datetime");

        builder.HasMany(r => r.Cases)
            .WithOne(c => c.Region)
            .HasForeignKey(c => c.RegionId);

        builder.HasMany(r => r.Files)
            .WithOne(f => f.Region)
            .HasForeignKey(f => f.RegionId);

        builder.HasMany(r => r.RegionsUsers)
            .WithOne(ru => ru.Region)
            .HasForeignKey(ru => ru.RegionId);
    }
}
