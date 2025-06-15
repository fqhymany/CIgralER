// BackEnd/Infrastructure/Hubs/GuestChatHub.cs
using System.Collections.Concurrent;
using LawyerProject.Application.Chats.DTOs;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Domain.Entities;
using LawyerProject.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LawyerProject.Infrastructure.Hubs;

[AllowAnonymous] // مهم: اجازه دسترسی بدون احراز هویت
public class GuestChatHub : Hub
{
    private readonly IApplicationDbContext _context;
    private readonly IChatHubService _chatHubService;
    private static readonly ConcurrentDictionary<string, string> _guestConnections = new();

    public GuestChatHub(IApplicationDbContext context, IChatHubService chatHubService)
    {
        _context = context;
        _chatHubService = chatHubService;
    }

    public override async Task OnConnectedAsync()
    {
        // Get session ID from query string or header
        var httpContext = Context.GetHttpContext();
        var sessionId = httpContext?.Request.Query["access_token"].ToString()
            ?? httpContext?.Request.Headers["X-Session-Id"].ToString();

        if (string.IsNullOrEmpty(sessionId))
        {
            Context.Abort();
            return;
        }

        // Verify guest session
        var guestUser = await _context.GuestUsers
            .FirstOrDefaultAsync(g => g.SessionId == sessionId);

        if (guestUser == null)
        {
            // Create new guest user if not exists
            guestUser = new GuestUser
            {
                SessionId = sessionId,
                IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                UserAgent = httpContext?.Request.Headers["User-Agent"].ToString(),
                LastActivityAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.GuestUsers.Add(guestUser);
            await _context.SaveChangesAsync(CancellationToken.None);
        }
        else
        {
            // Update last activity
            guestUser.LastActivityAt = DateTime.UtcNow;
            guestUser.IsActive = true;
            await _context.SaveChangesAsync(CancellationToken.None);
        }

        // Store connection mapping
        _guestConnections[Context.ConnectionId] = sessionId;

        // Join existing chat rooms for this guest
        var guestChatRooms = await _context.ChatRooms
            .Where(cr => cr.GuestIdentifier == sessionId)
            .Select(cr => cr.Id)
            .ToListAsync();

        foreach (var roomId in guestChatRooms)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_guestConnections.TryRemove(Context.ConnectionId, out var sessionId))
        {
            var guestUser = await _context.GuestUsers
                .FirstOrDefaultAsync(g => g.SessionId == sessionId);

            if (guestUser != null)
            {
                guestUser.LastActivityAt = DateTime.UtcNow;
                guestUser.IsActive = false;
                await _context.SaveChangesAsync(CancellationToken.None);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinRoom(string roomId)
    {
        if (!_guestConnections.TryGetValue(Context.ConnectionId, out var sessionId))
            return;

        // Verify guest has access to this room
        var hasAccess = await _context.ChatRooms
            .AnyAsync(cr => cr.Id.ToString() == roomId && cr.GuestIdentifier == sessionId);

        if (hasAccess)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        }
    }

    public async Task SendMessage(int chatRoomId, string content)
    {
        if (!_guestConnections.TryGetValue(Context.ConnectionId, out var sessionId))
            return;

        var guestUser = await _context.GuestUsers
            .FirstOrDefaultAsync(g => g.SessionId == sessionId);

        if (guestUser == null)
            return;

        // Verify guest has access to this room
        var chatRoom = await _context.ChatRooms
            .FirstOrDefaultAsync(cr => cr.Id == chatRoomId && cr.GuestIdentifier == sessionId);

        if (chatRoom == null)
            return;

        // Create message
        var message = new ChatMessage
        {
            Content = content,
            ChatRoomId = chatRoomId,
            Type = MessageType.Text,
            // SenderId is null for guests
        };

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync(CancellationToken.None);

        // Create DTO
        var messageDto = new ChatMessageDto(
            message.Id,
            message.Content,
            null, // No SenderId for guests
            guestUser.Name ?? "Guest",
            guestUser.Name ?? "Guest",
            null, // No avatar
            message.ChatRoomId,
            message.Type,
            null, // No attachment
            null, // No reply
            message.Created,
            false,
            null,
            null,
            null,
            null,
            new List<ReactionInfo>()
        );

        // Broadcast to room
        await Clients.Group(chatRoomId.ToString()).SendAsync("ReceiveMessage", messageDto);
    }

    public async Task StartTyping(string roomId)
    {
        if (!_guestConnections.TryGetValue(Context.ConnectionId, out var sessionId))
            return;

        var guestUser = await _context.GuestUsers
            .FirstOrDefaultAsync(g => g.SessionId == sessionId);

        if (guestUser != null)
        {
            var typingIndicator = new TypingIndicatorDto
            (
                 null,
                 guestUser.Name ?? "Guest",
                 int.Parse(roomId),
                 true
            );

            await Clients.OthersInGroup(roomId).SendAsync("UserTyping", typingIndicator);
        }
    }

    public async Task StopTyping(string roomId)
    {
        if (!_guestConnections.TryGetValue(Context.ConnectionId, out var sessionId))
            return;

        var guestUser = await _context.GuestUsers
            .FirstOrDefaultAsync(g => g.SessionId == sessionId);

        if (guestUser != null)
        {
            var typingIndicator = new TypingIndicatorDto
            (
                 null,
                 guestUser.Name ?? "Guest",
                 int.Parse(roomId),
                 false
            );

            await Clients.OthersInGroup(roomId).SendAsync("UserTyping", typingIndicator);
        }
    }
}
