﻿using AutoMapper; // <<< اضافه کنید
using LawyerProject.Application.Chats.DTOs;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Domain.Entities;
using LawyerProject.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LawyerProject.Application.Chats.Commands;

public record ForwardMessageCommand(int OriginalMessageId, int TargetChatRoomId) : IRequest<ChatMessageDto>;

public class ForwardMessageCommandHandler : IRequestHandler<ForwardMessageCommand, ChatMessageDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IChatHubService _chatHubService;
    private readonly IMapper _mapper; // <<< ۱. تزریق IMapper

    public ForwardMessageCommandHandler(IApplicationDbContext context, IUser user, IChatHubService chatHubService, IMapper mapper) // <<< ۲. اضافه کردن به کانستراکتور
    {
        _context = context;
        _user = user;
        _chatHubService = chatHubService;
        _mapper = mapper; // <<< ۳. مقداردهی اولیه
    }

    public async Task<ChatMessageDto> Handle(ForwardMessageCommand request, CancellationToken cancellationToken)
    {
        var forwarderUserId = _user.Id ?? throw new UnauthorizedAccessException("User not authenticated.");

        // --- بخش ۱: خواندن اطلاعات اولیه ---
        var originalMessage = await _context.ChatMessages
            .AsNoTracking()
            .Include(m => m.Sender)
            .FirstOrDefaultAsync(m => m.Id == request.OriginalMessageId, cancellationToken)
            ?? throw new KeyNotFoundException($"Original message with Id {request.OriginalMessageId} not found.");

        var targetChatRoom = await _context.ChatRooms
            .AsNoTracking()
            .Include(cr => cr.Members).ThenInclude(m => m.User) // Include کامل برای مپینگ
            .FirstOrDefaultAsync(cr => cr.Id == request.TargetChatRoomId, cancellationToken)
            ?? throw new KeyNotFoundException($"Target chat room with Id {request.TargetChatRoomId} not found.");

        if (!targetChatRoom.Members.Any(m => m.UserId == forwarderUserId))
            throw new UnauthorizedAccessException("User is not a member of the target chat room.");

        // --- بخش ۲: ایجاد پیام فوروارد شده ---
        string forwardedContent = $"[هدایت شده از: {originalMessage.Sender.FirstName} {originalMessage.Sender.LastName}]\n{originalMessage.Content}";

        var forwardedMessage = new ChatMessage
        {
            Content = forwardedContent,
            SenderId = forwarderUserId,
            ChatRoomId = request.TargetChatRoomId,
            Type = originalMessage.Type,
            AttachmentUrl = originalMessage.AttachmentUrl,
            Created = DateTime.UtcNow
        };

        _context.ChatMessages.Add(forwardedMessage);
        await _context.SaveChangesAsync(cancellationToken);

        // --- بخش ۳: ساخت DTO و ارسال نوتیفیکیشن پیام جدید ---

        // پیام را مجدداً با Sender آن می‌خوانیم تا بتوانیم به DTO مپ کنیم
        var messageToNotify = await _context.ChatMessages
            .AsNoTracking()
            .Include(m => m.Sender)
            .FirstAsync(m => m.Id == forwardedMessage.Id, cancellationToken);

        var messageDto = _mapper.Map<ChatMessageDto>(messageToNotify);
        await _chatHubService.SendMessageToRoom(request.TargetChatRoomId.ToString(), messageDto);

        // --- بخش ۴: آپدیت و ارسال وضعیت جدید چت‌روم ---
        foreach (var member in targetChatRoom.Members)
        {
            if (string.IsNullOrEmpty(member.UserId)) continue;

            var roomUpdateDto = _mapper.Map<ChatRoomDto>(targetChatRoom);

            // سفارشی‌سازی DTO برای هر کاربر
            roomUpdateDto.UnreadCount = await _context.ChatMessages
                .CountAsync(m => m.ChatRoomId == request.TargetChatRoomId &&
                                 m.SenderId != member.UserId &&
                                 m.Id > (member.LastReadMessageId ?? 0), cancellationToken);

            // آپدیت آخرین پیام
            var forwarderUser = targetChatRoom.Members.First(m => m.UserId == forwarderUserId).User;
            roomUpdateDto.LastMessageContent = forwardedMessage.Content;
            roomUpdateDto.LastMessageTime = forwardedMessage.Created;
            roomUpdateDto.LastMessageSenderName = $"{forwarderUser.FirstName} {forwarderUser.LastName}";

            if (!targetChatRoom.IsGroup)
            {
                var otherUser = targetChatRoom.Members.FirstOrDefault(m => m.UserId != member.UserId)?.User;
                if (otherUser != null)
                {
                    roomUpdateDto.Name = $"{otherUser.FirstName} {otherUser.LastName}";
                    roomUpdateDto.Avatar = otherUser.Avatar;
                }
            }

            await _chatHubService.SendChatRoomUpdateToUser(member.UserId, roomUpdateDto);
        }

        return messageDto; // بازگرداندن DTO پیام فوروارد شده
    }
}
