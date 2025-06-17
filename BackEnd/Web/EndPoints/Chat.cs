using System.Linq;
using System.Security.Claims;
using LawyerProject.Application.Chats.Commands;
using LawyerProject.Application.Chats.DTOs;
using LawyerProject.Application.Chats.Queries;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Domain.Entities;
using LawyerProject.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LawyerProject.Web.Endpoints;

public class Chat : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var chatApi = app.MapGroup("/api/chat").RequireAuthorization();

        // Chat Room endpoints
        chatApi.MapGet("/rooms", GetChatRooms);
        chatApi.MapGet("/rooms/{roomId:int}/messages", GetChatMessages);
        chatApi.MapPost("/rooms", CreateChatRoom);
        chatApi.MapPost("/rooms/{roomId:int}/join", JoinChatRoom);
        chatApi.MapDelete("/rooms/{roomId:int}/leave", LeaveChatRoom);
        chatApi.MapGet("/rooms/{roomId:int}/members", GetChatRoomMembers);
        chatApi.MapGet("/rooms/{roomId}/unread-count", GetUnreadCount);
        // Message endpoints
        chatApi.MapPost("/rooms/{roomId:int}/messages", SendMessage);
        chatApi.MapPut("/messages/{messageId:int}", EditMessage).WithName("EditChatMessage");
        chatApi.MapDelete("/messages/{messageId:int}", DeleteMessage).WithName("DeleteChatMessage");
        chatApi.MapPost("/messages/{messageId:int}/react", ReactToMessage).WithName("ReactToChatMessage");
        chatApi.MapPost("/messages/forward", ForwardMessage).WithName("ForwardChatMessage");

        // User endpoints
        chatApi.MapGet("/users/online", GetOnlineUsers);
        chatApi.MapGet("/users/search", SearchUsers);
    }

    private static async Task<IResult> GetUnreadCount(
        int roomId,
        IApplicationDbContext context,
        IUser user)
    {
        var userId = user.Id;

        var lastReadMessage = await context.ChatRoomMembers
            .Where(m => m.UserId == userId && m.ChatRoomId == roomId)
            .Select(m => m.LastReadMessageId)
            .FirstOrDefaultAsync();

        var unreadCount = await context.ChatMessages
            .Where(m => m.ChatRoomId == roomId &&
                        m.SenderId != userId &&
                        !m.IsDeleted &&
                        (lastReadMessage == null || m.Id > lastReadMessage))
            .CountAsync();

        return Results.Ok(new { UnreadCount = unreadCount });
    }

    private static async Task<IResult> GetChatRooms(
    IApplicationDbContext context,
    IUser user,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
    {
        var userId = user.Id;

        var query = from room in context.ChatRooms
                    join member in context.ChatRoomMembers on room.Id equals member.ChatRoomId
                    where member.UserId == userId
                    select new
                    {
                        Room = room,
                        Member = member,
                        LastMessage = context.ChatMessages
                            .Where(m => m.ChatRoomId == room.Id && !m.IsDeleted)
                            .OrderByDescending(m => m.Created)
                            .FirstOrDefault(),
                        UnreadCount = context.ChatMessages
                            .Count(m => m.ChatRoomId == room.Id &&
                                        m.SenderId != userId &&
                                        !m.IsDeleted &&
                                        (member.LastReadMessageId == null || m.Id > member.LastReadMessageId))
                    };

        var rooms = await query
            .OrderByDescending(x => x.LastMessage != null ? x.LastMessage.Created : x.Room.Created)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ChatRoomDto(
                x.Room.Id,
                x.Room.Name,
                x.Room.Description,
                x.Room.IsGroup,
                x.Room.Avatar,
                x.Room.Created,
                x.Room.ChatRoomType, // اضافه کردن ChatRoomType
                context.ChatMessages.Count(m => m.ChatRoomId == x.Room.Id),
                x.LastMessage != null ? x.LastMessage.Content : null,
                x.LastMessage != null ? x.LastMessage.Created : null,
                (x.LastMessage != null ? x.LastMessage.SenderId : null)!,
                x.UnreadCount
            ))
            .ToListAsync();

        return Results.Ok(rooms);
    }

    private static async Task<IResult> GetChatMessages(
        int roomId,
        int page,
        int pageSize,
        IMediator mediator)
    {
        var query = new GetChatMessagesQuery(roomId, page, pageSize);
        var result = await mediator.Send(query);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateChatRoom(
        CreateChatRoomRequest request,
        IMediator mediator)
    {
        var command = new CreateChatRoomCommand(
            request.Name,
            request.Description,
            request.IsGroup,
            request.MemberIds,
            request.RegionId
        );
        var result = await mediator.Send(command);
        return Results.Created($"/api/chat/rooms/{result.Id}", result);
    }

    private static async Task<IResult> SendMessage(
        int roomId,
        SendMessageRequest request,
        IMediator mediator)
    {
        var command = new SendMessageCommand(
            roomId,
            request.Content,
            request.Type,
            request.AttachmentUrl,
            request.ReplyToMessageId
        );
        var result = await mediator.Send(command);
        return Results.Created($"/api/chat/messages/{result.Id}", result);
    }

    private static async Task<IResult> JoinChatRoom(
        int roomId,
        IApplicationDbContext context,
        HttpContext httpContext,
        IUser user)
    {
        var userId = user.Id;

        var existingMember = await context.ChatRoomMembers
            .FirstOrDefaultAsync(m => m.UserId == userId && m.ChatRoomId == roomId);

        if (existingMember != null)
            return Results.BadRequest("Already a member");

        var member = new ChatRoomMember
        {
            UserId = userId,
            ChatRoomId = roomId,
            Role = ChatRole.Member
        };

        context.ChatRoomMembers.Add(member);
        await context.SaveChangesAsync(CancellationToken.None);

        return Results.Ok();
    }

    private static async Task<IResult> LeaveChatRoom(
        int roomId,
        IApplicationDbContext context,
        HttpContext httpContext,
        IUser user)
    {
        var userId = user.Id;

        var member = await context.ChatRoomMembers
            .FirstOrDefaultAsync(m => m.UserId == userId && m.ChatRoomId == roomId);

        if (member == null)
            return Results.NotFound();

        context.ChatRoomMembers.Remove(member);
        await context.SaveChangesAsync(CancellationToken.None);

        return Results.Ok();
    }

    private static async Task<IResult> GetOnlineUsers(
        IApplicationDbContext context,
        IUser user)
    {
        var currentUserId = user.Id;
        var onlineUsers = await context.UserConnections
            .Where(c => c.IsActive && c.UserId != currentUserId)
            .Include(c => c.User)
            .Select(c => new
            {
                c.User.Id,
                UserFullName = c.User.FirstName + " " + c.User.LastName,
                c.User.Avatar
            })
            .Distinct()
            .ToListAsync();

        return Results.Ok(onlineUsers);
    }

    private static async Task<IResult> SearchUsers(
        string query,
        IApplicationDbContext context,
        IUser currentUser)
    {
        var activeRegionId = currentUser.RegionId;
        var currentUserId = currentUser.Id;

        var users = await context.Users
            .Where(u => u.Id != currentUserId)
            .Where(u => u.RegionsUsers.Any(ru => ru.RegionId == activeRegionId))
            .Where(u => u.UserName!.Contains(query) ||
                       u.Email!.Contains(query) ||
                       (u.FirstName + " " + u.LastName).Contains(query))
            .Select(u => new
            {
                u.Id,
                UserName = u.UserName,
                FullName = u.FirstName + " " + u.LastName,
                u.Email,
                u.Avatar
            })
            .Take(20)
            .ToListAsync();

        return Results.Ok(users);
    }

    private static async Task<IResult> GetChatRoomMembers(
        int roomId,
        IApplicationDbContext context)
    {
        var members = await context.ChatRoomMembers
            .Where(m => m.ChatRoomId == roomId)
            .Include(m => m.User)
            .Select(m => new
            {
                m.User.Id,
                m.User.UserName,
                m.User.Avatar,
                m.Role,
                m.JoinedAt,
                m.LastSeenAt
            })
            .ToListAsync();

        return Results.Ok(members);
    }

    private static async Task<IResult> EditMessage(
        int messageId,
        EditMessageRequest request,
        IMediator mediator)
    {
        var command = new EditMessageCommand(messageId, request.NewContent);
        var result = await mediator.Send(command);
        return Results.Ok(result);
    }

    private static async Task<IResult> DeleteMessage(
        int messageId,
        IMediator mediator)
    {
        var command = new DeleteMessageCommand(messageId);
        await mediator.Send(command);
        return Results.Ok(new { MessageId = messageId, Status = "Deleted" });
    }

    private static async Task<IResult> ReactToMessage(
        int messageId,
        ReactRequest requestBody,
        IMediator mediator)
    {
        var command = new ReactToMessageCommand(messageId, requestBody.Emoji);
        var result = await mediator.Send(command);
        return Results.Ok(result);
    }

    private static async Task<IResult> ForwardMessage(
        ForwardMessageRequest requestBody,
        IMediator mediator)
    {
        var command = new ForwardMessageCommand(requestBody.OriginalMessageId, requestBody.TargetChatRoomId);
        var result = await mediator.Send(command);
        return Results.Ok(result);
    }

    // Request DTOs
    public record CreateChatRoomRequest(
        string Name,
        string? Description,
        bool IsGroup,
        List<string>? MemberIds = null,
        int? RegionId = null
    );

    public record SendMessageRequest(
        string Content,
        MessageType Type = MessageType.Text,
        string? AttachmentUrl = null,
        int? ReplyToMessageId = null
    );

    public record EditMessageRequest(string NewContent);

    public record ReactRequest(string Emoji);

    public record ForwardMessageRequest(int OriginalMessageId, int TargetChatRoomId);
}
