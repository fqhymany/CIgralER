using LawyerProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawyerProject.Infrastructure.Data.Configurations;

public class UsersRoleConfiguration : IEntityTypeConfiguration<UsersRole>
{
    public void Configure(EntityTypeBuilder<UsersRole> builder)
    {
        builder.ToTable("UsersRoles");

        builder.HasOne(d => d.Role).WithMany(p => p.UsersRoles)
            .HasForeignKey(d => d.RoleId)
            .HasConstraintName("FK__UserAcces__Acces__3E1D39E1");

        builder.HasOne(d => d.Region).WithMany(p => p.UsersRoles)
            .HasForeignKey(d => d.RegionId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_UsersRoles_Regions");
    }
}
