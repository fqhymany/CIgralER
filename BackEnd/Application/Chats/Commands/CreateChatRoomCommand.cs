using System.Security.Claims;
using LawyerProject.Application.Chats.DTOs;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Domain.Entities;
using LawyerProject.Domain.Enums;
using Microsoft.AspNetCore.Http;
using AutoMapper; // <<< اضافه کنید
using Microsoft.EntityFrameworkCore; // <<< اضافه کنید

namespace LawyerProject.Application.Chats.Commands;

public record CreateChatRoomCommand(
    string Name,
    string? Description,
    bool IsGroup,
    List<string>? MemberIds = null,
    int? RegionId = null,
    string? GuestFullName = null,
    string? GuestEmail = null
) : IRequest<ChatRoomDto>;

public class CreateChatRoomCommandHandler : IRequestHandler<CreateChatRoomCommand, ChatRoomDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IChatHubService _chatHubService;
    private readonly IMapper _mapper; // <<< ۱. تزریق IMapper

    public CreateChatRoomCommandHandler(IApplicationDbContext context, IUser user, IChatHubService chatHubService, IMapper mapper) // <<< ۲. اضافه کردن به کانستراکتور
    {
        _context = context;
        _user = user;
        _chatHubService = chatHubService;
        _mapper = mapper; // <<< ۳. مقداردهی اولیه
    }

    public async Task<ChatRoomDto> Handle(CreateChatRoomCommand request, CancellationToken cancellationToken)
    {
        var creatorUserId = _user.Id ?? throw new UnauthorizedAccessException("User is not authenticated.");
        var activeRegionId = _user.RegionId;

        // =================================================================
        // بخش ۱: بررسی وجود چت خصوصی از قبل
        // =================================================================
        if (!request.IsGroup && request.MemberIds?.Count == 1)
        {
            var otherMemberId = request.MemberIds.First();
            if (creatorUserId == otherMemberId)
                throw new InvalidOperationException("Cannot create a private chat with yourself.");

            var existingRoom = await _context.ChatRooms
                .AsNoTracking()
                .Include(cr => cr.Members).ThenInclude(m => m.User)
                .FirstOrDefaultAsync(cr => !cr.IsGroup && cr.RegionId == activeRegionId &&
                                       cr.Members.All(m => m.UserId == creatorUserId || m.UserId == otherMemberId) &&
                                       cr.Members.Count == 2, cancellationToken);

            if (existingRoom != null)
            {
                // چت از قبل وجود دارد. فقط آن را به DTO تبدیل کرده و برگردانید
                var roomDto = _mapper.Map<ChatRoomDto>(existingRoom);
                var otherUser = existingRoom.Members.First(m => m.UserId != creatorUserId).User;

                // سفارشی‌سازی نام و آواتار برای نمایش در فرانت‌اند
                roomDto.Name = $"{otherUser.FirstName} {otherUser.LastName}";
                roomDto.Avatar = otherUser.Avatar;

                return roomDto;
            }
        }

        // =================================================================
        // بخش ۲: ایجاد چت‌روم جدید
        // =================================================================
        var chatRoom = new ChatRoom
        {
            Name = request.Name,
            Description = request.Description,
            IsGroup = request.IsGroup,
            CreatedById = creatorUserId,
            RegionId = activeRegionId,
        };

        // افزودن اعضا به چت‌روم
        var allMemberIds = new List<string>(request.MemberIds ?? new List<string>());
        if (!allMemberIds.Contains(creatorUserId))
        {
            allMemberIds.Add(creatorUserId);
        }

        foreach (var memberId in allMemberIds)
        {
            chatRoom.Members.Add(new ChatRoomMember
            {
                UserId = memberId,
                Role = (memberId == creatorUserId) ? ChatRole.Owner : ChatRole.Member
            });
        }

        _context.ChatRooms.Add(chatRoom);
        await _context.SaveChangesAsync(cancellationToken);

        // =================================================================
        // بخش ۳: خواندن مجدد و ارسال نوتیفیکیشن
        // =================================================================
        // برای اینکه تمام روابط (Members.User) را داشته باشیم، آن را مجدداً می‌خوانیم
        var newlyCreatedRoom = await _context.ChatRooms
            .AsNoTracking()
            .Include(r => r.Members).ThenInclude(m => m.User)
            .FirstAsync(r => r.Id == chatRoom.Id, cancellationToken);

        // برای هر عضو، یک DTO سفارشی ساخته و از طریق هاب ارسال می‌کنیم
        foreach (var member in newlyCreatedRoom.Members)
        {
            var roomDtoForMember = _mapper.Map<ChatRoomDto>(newlyCreatedRoom);

            if (!newlyCreatedRoom.IsGroup)
            {
                // برای چت خصوصی، نام و آواتار طرف مقابل را نمایش می‌دهیم
                var otherUser = newlyCreatedRoom.Members.FirstOrDefault(m => m.UserId != member.UserId)?.User;
                if (otherUser != null)
                {
                    roomDtoForMember.Name = $"{otherUser.FirstName} {otherUser.LastName}";
                    roomDtoForMember.Avatar = otherUser.Avatar;
                }
            }
            // برای چت گروهی، نام و آواتار خود گروه نمایش داده می‌شود که AutoMapper به درستی انجام داده است.

            if (member.UserId != null)
            {
                await _chatHubService.SendChatRoomUpdateToUser(member.UserId, roomDtoForMember);
            }
        }

        // DTO نهایی را برای بازگشت به فرانت‌اند ایجاد می‌کنیم
        var finalDtoForRequester = _mapper.Map<ChatRoomDto>(newlyCreatedRoom);
        if (!newlyCreatedRoom.IsGroup)
        {
            var otherUser = newlyCreatedRoom.Members.FirstOrDefault(m => m.UserId != creatorUserId)?.User;
            if (otherUser != null)
            {
                finalDtoForRequester.Name = $"{otherUser.FirstName} {otherUser.LastName}";
                finalDtoForRequester.Avatar = otherUser.Avatar;
            }
        }

        return finalDtoForRequester;
    }
}
