﻿using LawyerProject.Domain.Entities;
using LawyerProject.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawyerProject.Infrastructure.Data.Configurations;

public class ChatRoomConfiguration : IEntityTypeConfiguration<ChatRoom>
{
    public void Configure(EntityTypeBuilder<ChatRoom> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        entity.Property(e => e.Description).HasMaxLength(500);
        entity.Property(e => e.Avatar).HasMaxLength(500);
        entity.Property(e => e.ChatRoomType)
            .HasConversion<int>() // یا string
            .HasDefaultValue(ChatRoomType.UserToUser);

        entity.HasOne(e => e.CreatedBy)
            .WithMany()
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.Region)
            .WithMany()
            .HasForeignKey(e => e.RegionId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasIndex(e => e.Name);
        entity.HasIndex(e => e.CreatedById);
    }
}
