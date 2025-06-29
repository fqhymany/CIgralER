﻿using System.Linq;
using System.Security.Claims;
using AutoMapper;
using LawyerProject.Application.Chats.Commands;
using LawyerProject.Application.Chats.DTOs;
using LawyerProject.Application.Chats.Queries;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Domain.Entities;
using LawyerProject.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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

        // File upload endpoint
        chatApi.MapPost("/upload", UploadFile)
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data");
    }

    [IgnoreAntiforgeryToken]
    public async Task<Results<Ok<ChatFileUploadResult>, BadRequest<string>>> UploadFile(
        ISender sender,
        IFormFile file,
        [FromForm] int chatRoomId,
        [FromForm] MessageType type,
        HttpContext httpContext)
    {
        if (file == null || file.Length == 0)
            return TypedResults.BadRequest("No file was uploaded");

        var command = new UploadChatFileCommand
        {
            ChatRoomId = chatRoomId,
            File = file,
            Type = type
        };

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return TypedResults.BadRequest(result.Error ?? "Upload failed");

        return TypedResults.Ok(result.Data!);
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
    IMapper mapper, // <<< ۱. IMapper را به عنوان پارامتر اضافه کنید
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
    {
        var userId = user.Id;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        // ۲. کوئری شما بسیار قدرتمند است و بخش زیادی از آن را نگه می‌داریم
        // این کوئری با استفاده از select new به یک نوع ناشناس، از مشکل N+1 جلوگیری می‌کند
        var query = context.ChatRooms
            .Where(cr => cr.Members.Any(m => m.UserId == userId))
            .Select(room => new
            {
                // تمام اطلاعات مورد نیاز را در یک آبجکت ناشناس جمع‌آوری می‌کنیم
                RoomEntity = room,
                CurrentUserMembership = room.Members.FirstOrDefault(m => m.UserId == userId),
                OtherUser = !room.IsGroup ? room.Members.FirstOrDefault(m => m.UserId != userId)!.User : null,
                LastMessage = room.Messages
                    .Where(m => !m.IsDeleted)
                    .OrderByDescending(m => m.Created)
                    .FirstOrDefault()
            });

        // ۳. اجرای کوئری و دریافت نتایج
        var intermediateResults = await query
            .OrderByDescending(x => x.LastMessage != null ? x.LastMessage.Created : x.RoomEntity.Created)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // ۴. تبدیل نتایج میانی به DTO نهایی با کمک AutoMapper
        var finalDtoList = intermediateResults.Select(item =>
        {
            // ابتدا مپینگ پایه را با AutoMapper انجام می‌دهیم
            var dto = mapper.Map<ChatRoomDto>(item.RoomEntity);

            // سپس اطلاعات محاسبه‌شده و سفارشی را روی DTO تنظیم می‌کنیم
            dto.LastMessageContent = item.LastMessage?.Content;
            dto.LastMessageTime = item.LastMessage?.Created;
            dto.LastMessageSenderName = item.LastMessage?.Sender != null ? $"{item.LastMessage.Sender.FirstName} {item.LastMessage.Sender.LastName}" : null;

            dto.UnreadCount = context.ChatMessages.Count(m =>
                m.ChatRoomId == item.RoomEntity.Id &&
                m.SenderId != userId &&
                !m.IsDeleted &&
                (item.CurrentUserMembership!.LastReadMessageId == null || m.Id > item.CurrentUserMembership.LastReadMessageId)
            );

            // سفارشی‌سازی نام و آواتار برای چت‌های خصوصی
            if (!dto.IsGroup && item.OtherUser != null)
            {
                dto.Name = $"{item.OtherUser.FirstName} {item.OtherUser.LastName}";
                dto.Avatar = item.OtherUser.Avatar;
            }

            return dto;
        }).ToList();

        return Results.Ok(finalDtoList);
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
