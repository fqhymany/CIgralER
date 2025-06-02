using LawyerProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawyerProject.Infrastructure.Data.Configurations;

public class RegionsUserConfiguration : IEntityTypeConfiguration<RegionsUser>
{
    public void Configure(EntityTypeBuilder<RegionsUser> builder)
    {
        builder.HasKey(e => e.Id).HasName("PK__LawFirmU__3214EC07E90C24CF");

        builder.Property(e => e.Created)
            .HasDefaultValueSql("(getdate())")
            .HasColumnType("datetime");
        builder.Property(e => e.LastModified)
            .HasDefaultValueSql("(getdate())")
            .HasColumnType("datetime");

        builder.HasOne(d => d.Region)
            .WithMany(p => p.RegionsUsers)
            .HasForeignKey(d => d.RegionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_RegionsUsers_Regions");

        builder.HasOne(d => d.User)
            .WithMany(p => p.RegionsUsers)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_RegionsUsers_Users");
    }
}
