using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Domain.Entities;
using LawyerProject.Domain.Enums;

namespace LawyerProject.Application.Chats.Commands;

public record RemoveGroupMemberCommand(
    int ChatRoomId,
    string UserId
) : IRequest<bool>;

public class RemoveGroupMemberCommandHandler : IRequestHandler<RemoveGroupMemberCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IChatHubService _chatHubService;

    public RemoveGroupMemberCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IChatHubService chatHubService)
    {
        _context = context;
        _user = user;
        _chatHubService = chatHubService;
    }

    public async Task<bool> Handle(RemoveGroupMemberCommand request, CancellationToken cancellationToken)
    {
        // Check permissions
        var requesterMember = await _context.ChatRoomMembers
            .FirstOrDefaultAsync(m => m.ChatRoomId == request.ChatRoomId
                && m.UserId == _user.Id
                && (m.Role == ChatRole.Admin || m.Role == ChatRole.Owner),
                cancellationToken);

        if (requesterMember == null)
        {
            // Check if user is removing themselves
            if (request.UserId != _user.Id)
                throw new UnauthorizedAccessException("Only admins can remove members");
        }

        var memberToRemove = await _context.ChatRoomMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.ChatRoomId == request.ChatRoomId && m.UserId == request.UserId,
                cancellationToken);

        if (memberToRemove == null)
            return false;

        // Can't remove owner
        if (memberToRemove.Role == ChatRole.Owner && request.UserId != _user.Id)
            throw new InvalidOperationException("Cannot remove group owner");

        _context.ChatRoomMembers.Remove(memberToRemove);

        // System message
        var systemMessage = new ChatMessage
        {
            Content = request.UserId == _user.Id
                ? $"{memberToRemove.User.FirstName} {memberToRemove.User.LastName} left the group"
                : $"{memberToRemove.User.FirstName} {memberToRemove.User.LastName} was removed from the group",
            ChatRoomId = request.ChatRoomId,
            Type = MessageType.System
        };
        _context.ChatMessages.Add(systemMessage);

        await _context.SaveChangesAsync(cancellationToken);

        // Notify removed user
        await _chatHubService.SendMessageUpdateToRoom(
            request.ChatRoomId.ToString(),
            new { Action = "UserRemoved", UserId = request.UserId },
            "GroupMemberRemoved");

        return true;
    }
}
