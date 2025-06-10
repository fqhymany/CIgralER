using LawyerProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawyerProject.Infrastructure.Data.Configurations;

public class EncryptedFileMetadataConfiguration : IEntityTypeConfiguration<EncryptedFileMetadata>
{
    public void Configure(EntityTypeBuilder<EncryptedFileMetadata> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FileName).IsRequired();
        builder.Property(e => e.FilePath).IsRequired();
        builder.Property(e => e.FileSize).IsRequired();
        builder.Property(e => e.EncryptionKeyId).IsRequired();
        builder.Property(e => e.UploadedDate)
            .HasDefaultValueSql("(getdate())")
            .HasColumnType("datetime");

        // Relationships
        builder.HasOne(d => d.Case)
            .WithMany(p => p.Files)
            .HasForeignKey(d => d.CaseId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_EncryptedFileMetadata_Cases");

        builder.HasOne(d => d.Region)
            .WithMany(p => p.Files)
            .HasForeignKey(d => d.RegionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_EncryptedFileMetadata_Regions");

        builder.HasOne(d => d.UploadedBy)
            .WithMany(p => p.UploadedFiles)
            .HasForeignKey(d => d.UploadedById)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_EncryptedFileMetadata_Users");
    }
}
