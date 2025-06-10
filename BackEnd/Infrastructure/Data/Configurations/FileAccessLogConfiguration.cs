using LawyerProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawyerProject.Infrastructure.Data.Configurations
{
    public class FileAccessLogConfiguration : IEntityTypeConfiguration<FileAccessLog>
    {
        public void Configure(EntityTypeBuilder<FileAccessLog> builder)
        {
            builder.HasOne(f => f.User)
                   .WithMany()
                   .HasForeignKey(f => f.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
