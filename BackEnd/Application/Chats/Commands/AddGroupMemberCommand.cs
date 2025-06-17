using LawyerProject.Application.Chats.DTOs;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Domain.Entities;
using LawyerProject.Domain.Enums;

namespace LawyerProject.Application.Chats.Commands;

public record AddGroupMemberCommand(
    int ChatRoomId,
    List<string> UserIds,
    ChatRole Role = ChatRole.Member
) : IRequest<bool>;

public class AddGroupMemberCommandHandler : IRequestHandler<AddGroupMemberCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IChatHubService _chatHubService;

    public AddGroupMemberCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IChatHubService chatHubService)
    {
        _context = context;
        _user = user;
        _chatHubService = chatHubService;
    }

    public async Task<bool> Handle(AddGroupMemberCommand request, CancellationToken cancellationToken)
    {
        // Check if requester is admin/owner
        var requesterMember = await _context.ChatRoomMembers
            .FirstOrDefaultAsync(m => m.ChatRoomId == request.ChatRoomId
                && m.UserId == _user.Id
                && (m.Role == ChatRole.Admin || m.Role == ChatRole.Owner),
                cancellationToken);

        if (requesterMember == null)
            throw new UnauthorizedAccessException("Only admins can add members");

        var chatRoom = await _context.ChatRooms
            .Include(cr => cr.Members)
            .FirstOrDefaultAsync(cr => cr.Id == request.ChatRoomId, cancellationToken);

        if (chatRoom == null || !chatRoom.IsGroup)
            return false;

        foreach (var userId in request.UserIds)
        {
            // Check if already member
            if (chatRoom.Members.Any(m => m.UserId == userId))
                continue;

            var member = new ChatRoomMember
            {
                UserId = userId,
                ChatRoomId = request.ChatRoomId,
                Role = request.Role
            };
            _context.ChatRoomMembers.Add(member);

            // System message
            var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
            var systemMessage = new ChatMessage
            {
                Content = $"{user?.FirstName} {user?.LastName} was added to the group",
                ChatRoomId = request.ChatRoomId,
                Type = MessageType.System
            };
            _context.ChatMessages.Add(systemMessage);
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Update room for all members
        var roomDto = await GetUpdatedRoomDto(request.ChatRoomId, cancellationToken);
        foreach (var member in chatRoom.Members)
        {
            await _chatHubService.SendChatRoomUpdateToUser(member.UserId!, roomDto);
        }

        return true;
    }

    private async Task<ChatRoomDto> GetUpdatedRoomDto(int roomId, CancellationToken cancellationToken)
    {
        // Implementation to get updated room DTO
        var room = await _context.ChatRooms
            .Include(r => r.Messages)
            .FirstOrDefaultAsync(r => r.Id == roomId, cancellationToken);

        return new ChatRoomDto(
            room!.Id,
            room.Name,
            room.Description,
            room.IsGroup,
            room.Avatar,
            room.Created,
            room.ChatRoomType, 
            room.Messages.Count,
            room.Messages.LastOrDefault()?.Content,
            room.Messages.LastOrDefault()?.Created,
            null,
            0
        );
    }
}
