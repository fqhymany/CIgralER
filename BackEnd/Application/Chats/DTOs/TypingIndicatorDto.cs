namespace LawyerProject.Application.Chats.DTOs;

public record TypingIndicatorDto(
    string? UserId,
    string UserFullName,
    int ChatRoomId,
    bool IsTyping
);
