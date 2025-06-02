﻿using LawyerProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LawyerProject.Infrastructure.Data.Configurations;

public class ChatMessageConfiguration: IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Content).IsRequired();
        entity.Property(e => e.AttachmentUrl).HasMaxLength(1000);
        entity.Property(e => e.AttachmentType).HasMaxLength(100);

        entity.HasOne(e => e.Sender)
            .WithMany()
            .HasForeignKey(e => e.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(e => e.ChatRoom)
            .WithMany(c => c.Messages)
            .HasForeignKey(e => e.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.ReplyToMessage)
            .WithMany(m => m.Replies)
            .HasForeignKey(e => e.ReplyToMessageId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(e => e.ChatRoomId);
        entity.HasIndex(e => e.SenderId);
        entity.HasIndex(e => e.Created);
    }
}
