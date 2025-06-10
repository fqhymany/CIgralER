using LawyerProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawyerProject.Infrastructure.Data.Configurations;

public class UserConfiguration: IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(e => e.Id).HasName("PK__Users__3214EC07DF330207");

        builder.HasIndex(e => e.Email, "UQ__Users__A9D10534F4F8BD0E").IsUnique();

        builder.Property(e => e.Created)
            .HasDefaultValueSql("(getdate())")
            .HasColumnType("datetime");

        builder.Property(e => e.Email).HasMaxLength(100);
        builder.Property(e => e.FirstName).HasMaxLength(100);
        builder.Property(e => e.IsActive).HasDefaultValue(true);
        builder.Property(e => e.LastName).HasMaxLength(100);
        builder.Property(e => e.NationalCode).HasMaxLength(20);
        builder.Property(e => e.PasswordHash).HasMaxLength(255);

        builder.Property(e => e.LastModified)
            .HasDefaultValueSql("(getdate())")
            .HasColumnType("datetime");

        builder.Property(e => e.UserName).HasMaxLength(255);

        builder.HasMany(e => e.RegionsUsers)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.UploadedFiles)
            .WithOne(e => e.UploadedBy)
            .HasForeignKey(e => e.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
