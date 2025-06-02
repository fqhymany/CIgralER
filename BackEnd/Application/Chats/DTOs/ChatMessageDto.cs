using LawyerProject.Domain.Enums;

namespace LawyerProject.Application.Chats.DTOs;

public record ChatMessageDto(
    int Id,
    string Content,
    string? SenderId,
    string SenderName,
    string? SenderFullName,
    string? SenderAvatar,
    int ChatRoomId,
    MessageType Type,
    string? AttachmentUrl,
    int? ReplyToMessageId,
    DateTime CreatedAt,
    bool IsEdited,
    DateTime? EditedAt,
    string? RepliedMessageContent,
    string? RepliedMessageSenderFullName,
    MessageType? RepliedMessageType,
    List<ReactionInfo>? Reactions 
);
public record ReactionInfo(string Emoji, string UserId, string UserFullName);
