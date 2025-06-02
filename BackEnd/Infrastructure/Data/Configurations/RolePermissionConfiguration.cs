using LawyerProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawyerProject.Infrastructure.Data.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");

        builder.HasKey(e => e.Id).HasName("PK__AccessGr__3214EC0752A1A230");

        builder.Property(e => e.CanCreate).HasDefaultValue(false);
        builder.Property(e => e.CanDelete).HasDefaultValue(false);
        builder.Property(e => e.CanEdit).HasDefaultValue(false);
        builder.Property(e => e.CanView).HasDefaultValue(false);
        builder.Property(e => e.Section).HasMaxLength(100);

        builder.HasOne(d => d.Role).WithMany(p => p.RolePermissions)
            .HasForeignKey(d => d.RoleId)
            .HasConstraintName("FK__AccessGro__Acces__3864608B");
    }
}
