using LawyerProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawyerProject.Infrastructure.Data.Configurations;

public class GuestUserConfiguration: IEntityTypeConfiguration<GuestUser>
{
    public void Configure(EntityTypeBuilder<GuestUser> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Name).HasMaxLength(256);
        builder.HasIndex(e => new { e.Email, e.PhoneNumber }).IsUnique();
    }
}
