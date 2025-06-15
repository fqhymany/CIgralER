using LawyerProject.Application.Chats.DTOs;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Support.Commands;
using LawyerProject.Domain.Entities;
using LawyerProject.Domain.Enums;
using LawyerProject.Web.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LawyerProject.Web.EndPoints;

public class Support : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/support")
            .WithTags("Support");

        // Guest endpoints (no auth required)
        group.MapPost("/start", StartSupportChat)
            .AllowAnonymous()
            .WithName("StartSupportChat")
            .Produces<StartSupportChatResult>(StatusCodes.Status200OK)
            .RequireCors("ReactApp");

        group.MapPost("/guest/message", SendGuestMessage)
            .AllowAnonymous()
            .RequireHeader("X-Session-Id");

        // Agent endpoints (require auth)
        group.MapGet("/tickets", GetAgentTickets)
            .RequireAuthorization("Agent");

        group.MapPost("/tickets/{ticketId}/transfer", TransferTicket)
            .RequireAuthorization("Agent");

        group.MapPost("/agent/status", UpdateAgentStatus)
            .RequireAuthorization("Agent");

        group.MapGet("/agents/available", GetAvailableAgents)
            .RequireAuthorization("Agent");

        group.MapPost("/tickets/{ticketId}/close", CloseTicket)
            .RequireAuthorization("Agent");
    }

    private static async Task<IResult> StartSupportChat(
        HttpContext context,
        StartSupportChatRequest request,
        IMediator mediator)
    {
        // Get IP address
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        // Get or create session ID for guests
        var userId = context.User?.Identity?.IsAuthenticated == true
            ? context.User.FindFirst("sub")?.Value
            : null;

        var command = new StartSupportChatCommand(
            userId,
            request.GuestSessionId,
            request.GuestName,
            request.GuestEmail,
            request.GuestPhone,
            ipAddress,
            request.UserAgent ?? context.Request.Headers["User-Agent"].ToString(),
            request.InitialMessage ?? "New support chat started"
        );

        var result = await mediator.Send(command);
        return Results.Ok(result);
    }

    private static async Task<IResult> SendGuestMessage(
        HttpContext context,
        SendGuestMessageRequest request,
        IApplicationDbContext dbContext,
        IChatHubService chatHubService)
    {
        var sessionId = context.Request.Headers["X-Session-Id"].ToString();
        if (string.IsNullOrEmpty(sessionId))
            return Results.BadRequest("Session ID required");

        // Verify guest session
        var guestUser = await dbContext.GuestUsers
            .FirstOrDefaultAsync(g => g.SessionId == sessionId);

        if (guestUser == null)
            return Results.Unauthorized();

        // Update last activity
        guestUser.LastActivityAt = DateTime.UtcNow;

        // Create message
        var message = new ChatMessage
        {
            Content = request.Content,
            ChatRoomId = request.ChatRoomId,
            Type = request.Type,
            AttachmentUrl = request.AttachmentUrl
            // SenderId will be null for guests
        };

        dbContext.ChatMessages.Add(message);
        await dbContext.SaveChangesAsync(CancellationToken.None);

        // Broadcast via SignalR
        var messageDto = new ChatMessageDto(
            message.Id,
            message.Content,
            null,
            guestUser.Name ?? "Guest",
            guestUser.Name ?? "Guest",
            null,
            message.ChatRoomId,
            message.Type,
            message.AttachmentUrl,
            null,
            message.Created,
            false,
            null,
            null,
            null,
            null,
            new List<ReactionInfo>()
        );

        await chatHubService.SendMessageToRoom(
            request.ChatRoomId.ToString(),
            messageDto);

        return Results.Ok(messageDto);
    }

    private static async Task<IResult> GetAgentTickets(
        HttpContext context,
        IApplicationDbContext dbContext,
        [FromQuery] SupportTicketStatus? status)
    {
        var agentId = context.User.FindFirst("sub")?.Value;

        var query = dbContext.SupportTickets
            .Include(t => t.RequesterUser)
            .Include(t => t.RequesterGuest)
            .Include(t => t.ChatRoom)
                .ThenInclude(cr => cr.Messages)
            .Where(t => t.AssignedAgentUserId == agentId);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        var tickets = await query
            .OrderByDescending(t => t.Created)
            .Select(t => new
            {
                t.Id,
                t.Status,
                t.Created,
                t.ClosedAt,
                ChatRoomId = t.ChatRoomId,
                RequesterName = t.RequesterUser != null
                    ? $"{t.RequesterUser.FirstName} {t.RequesterUser.LastName}"
                    : t.RequesterGuest!.Name ?? "Guest",
                RequesterEmail = t.RequesterUser != null
                    ? t.RequesterUser.Email
                    : t.RequesterGuest!.Email,
                LastMessage = t.ChatRoom.Messages
                    .OrderByDescending(m => m.Created)
                    .Select(m => new
                    {
                        m.Content,
                        m.Created,
                        SenderName = m.Sender != null
                            ? $"{m.Sender.FirstName} {m.Sender.LastName}"
                            : "Guest"
                    })
                    .FirstOrDefault(),
                UnreadCount = t.ChatRoom.Messages
                    .Count(m => m.SenderId != agentId && m.Created > t.ChatRoom.Members
                        .Where(mem => mem.UserId == agentId)
                        .Select(mem => mem.LastSeenAt)
                        .FirstOrDefault())
            })
            .ToListAsync();

        return Results.Ok(tickets);
    }

    private static async Task<IResult> TransferTicket(
        int ticketId,
        TransferTicketRequest request,
        IMediator mediator)
    {
        var command = new TransferChatCommand(
            ticketId,
            request.NewAgentId,
            request.Reason
        );

        var result = await mediator.Send(command);
        return result ? Results.Ok() : Results.NotFound();
    }

    private static async Task<IResult> UpdateAgentStatus(
        HttpContext context,
        UpdateAgentStatusRequest request,
        IAgentAssignmentService agentService)
    {
        var agentId = context.User.FindFirst("sub")?.Value!;
        await agentService.UpdateAgentStatusAsync(agentId, request.Status);
        return Results.Ok();
    }

    private static async Task<IResult> GetAvailableAgents(
        IApplicationDbContext dbContext)
    {
        var agents = await dbContext.Users
            .Where(u => u.AgentStatus != null && u.AgentStatus != AgentStatus.Offline)
            .Select(u => new
            {
                u.Id,
                Name = $"{u.FirstName} {u.LastName}",
                u.AgentStatus,
                u.CurrentActiveChats,
                u.MaxConcurrentChats,
                WorkloadPercentage = u.MaxConcurrentChats > 0
                    ? (u.CurrentActiveChats ?? 0) * 100 / u.MaxConcurrentChats
                    : 0
            })
            .OrderBy(u => u.WorkloadPercentage)
            .ToListAsync();

        return Results.Ok(agents);
    }

    private static async Task<IResult> CloseTicket(
        int ticketId,
        CloseTicketRequest request,
        IApplicationDbContext dbContext,
        IChatHubService chatHubService)
    {
        var ticket = await dbContext.SupportTickets
            .Include(t => t.ChatRoom)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
            return Results.NotFound();

        ticket.Status = SupportTicketStatus.Closed;
        ticket.ClosedAt = DateTime.UtcNow;

        // Add closing message
        var closingMessage = new ChatMessage
        {
            Content = $"Chat closed: {request.Reason ?? "Resolved"}",
            ChatRoomId = ticket.ChatRoomId,
            Type = MessageType.System
        };
        dbContext.ChatMessages.Add(closingMessage);

        // Update agent active chats
        if (!string.IsNullOrEmpty(ticket.AssignedAgentUserId))
        {
            var agent = await dbContext.Users.FindAsync(ticket.AssignedAgentUserId);
            if (agent is { CurrentActiveChats: > 0 })
            {
                agent.CurrentActiveChats--;
                if (agent.AgentStatus == AgentStatus.Busy &&
                    agent.CurrentActiveChats < agent.MaxConcurrentChats)
                {
                    agent.AgentStatus = AgentStatus.Available;
                }
            }
        }

        await dbContext.SaveChangesAsync(CancellationToken.None);

        // Notify via SignalR
        await chatHubService.SendMessageUpdateToRoom(
            ticket.ChatRoomId.ToString(),
            new { TicketId = ticketId, Status = "Closed" },
            "TicketClosed");

        return Results.Ok();
    }

    // Request DTOs
    public record StartSupportChatRequest(
        string? GuestSessionId,
        string? GuestName,
        string? GuestEmail,
        string? GuestPhone,
        string? UserAgent,
        string? InitialMessage
    );

    public record SendGuestMessageRequest(
        int ChatRoomId,
        string Content,
        MessageType Type = MessageType.Text,
        string? AttachmentUrl = null
    );

    public record TransferTicketRequest(
        string NewAgentId,
        string? Reason
    );

    public record UpdateAgentStatusRequest(
        AgentStatus Status
    );

    public record CloseTicketRequest(
        string? Reason
    );
}
