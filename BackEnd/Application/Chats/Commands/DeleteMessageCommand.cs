// File: BackEnd/Commands/DeleteMessageCommand.cs
using LawyerProject.Application.Chats.DTOs;
using LawyerProject.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LawyerProject.Application.Chats.Commands;

public record DeleteMessageCommand(int MessageId) : IRequest<bool>; // Returns true if successful

public class DeleteMessageCommandHandler : IRequestHandler<DeleteMessageCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IChatHubService _chatHubService;

    public DeleteMessageCommandHandler(IApplicationDbContext context, IUser user, IChatHubService chatHubService)
    {
        _context = context;
        _user = user;
        _chatHubService = chatHubService;
    }

    public async Task<bool> Handle(DeleteMessageCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;
        var message = await _context.ChatMessages
                                .Include(m => m.ChatRoom) // Include ChatRoom
                                    .ThenInclude(cr => cr.Members) // To get members for room update
                                        .ThenInclude(crm => crm.User)
                                .Include(m => m.ChatRoom)
                                    .ThenInclude(cr => cr.Messages) // To find the new last message
                                        .ThenInclude(msg => msg.Sender)
                                .FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken);

        if (message == null)
        {
            throw new KeyNotFoundException("Message not found.");
        }

        if (message.SenderId != userId)
        {
            throw new UnauthorizedAccessException("You can only delete your own messages.");
        }

        var chatRoomId = message.ChatRoomId;
        var chatRoom = message.ChatRoom;

        // Soft delete: mark as deleted
        message.IsDeleted = true;
        message.Content = "[پیام حذف شد]";
        message.AttachmentUrl = null;
        // You might want to clear other fields like AttachmentType, FileName, FileSize if they exist on ChatMessage entity

        await _context.SaveChangesAsync(cancellationToken);

        // Notify clients that message is deleted
        await _chatHubService.SendMessageUpdateToRoom(chatRoomId.ToString(),
            new { MessageId = message.Id, ChatRoomId = chatRoomId, IsDeleted = true },
            "MessageDeleted");

        // ---- START: Update and Broadcast ChatRoom Info ----
        var updatedMessagesInRoom = await _context.ChatMessages
                                            .Where(m => m.ChatRoomId == chatRoomId && !m.IsDeleted)
                                            .OrderByDescending(m => m.Created)
                                            .Include(m => m.Sender)
                                            .ToListAsync(cancellationToken);

        var newLastMessage = updatedMessagesInRoom.FirstOrDefault();

        foreach (var member in chatRoom.Members)
        {
            if (string.IsNullOrEmpty(member.UserId) || member.User == null) continue;

            int unreadCount = updatedMessagesInRoom
                .Count(m => m.SenderId != member.UserId &&
                            m.Id > (member.LastReadMessageId ?? 0));

            string roomNameForMember = chatRoom.Name;
            string? roomAvatarForMember = chatRoom.Avatar;

            if (!chatRoom.IsGroup && chatRoom.Members.Count >= 2)
            {
                var otherUserInChat = chatRoom.Members.FirstOrDefault(m_other => m_other.UserId != member.UserId)?.User;
                if (otherUserInChat != null)
                {
                    roomNameForMember = $"{otherUserInChat.FirstName} {otherUserInChat.LastName}";
                    roomAvatarForMember = otherUserInChat.Avatar;
                }
            }
            var lastMessageSenderFullName = newLastMessage?.Sender != null ? $"{newLastMessage.Sender.FirstName} {newLastMessage.Sender.LastName}" : null;

            var roomUpdateDto = new ChatRoomDto(
                chatRoom.Id,
                roomNameForMember,
                chatRoom.Description,
                chatRoom.IsGroup,
                roomAvatarForMember,
                chatRoom.Created,
                updatedMessagesInRoom.Count, // Updated message count
                newLastMessage?.Content.Length > 50 ? newLastMessage.Content.Substring(0, 50) + "..." : newLastMessage?.Content,
                newLastMessage?.Created,
                lastMessageSenderFullName, // Use FullName
                unreadCount
            );
            await _chatHubService.SendChatRoomUpdateToUser(member.UserId, roomUpdateDto);
        }
        // ---- END: Update and Broadcast ChatRoom Info ----

        return true;
    }

}
