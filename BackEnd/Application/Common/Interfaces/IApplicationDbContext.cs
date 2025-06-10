using LawyerProject.Domain.Entities;
using LawyerProject.Domain.Entities.BankEntities;
using LawyerProject.Domain.Entities.CaseFinancials;
using Microsoft.EntityFrameworkCore;

namespace LawyerProject.Application.Common.Interfaces;
public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }
    DbSet<Case> Cases { get; }
    DbSet<User> Users { get; }
    DbSet<RegionsUser> RegionsUsers { get; }
    DbSet<Region> Regions { get; }
    DbSet<UsersRole> UsersRoles { get; }
    DbSet<Role> Roles { get; }
    DbSet<CaseType> CaseTypes { get;}
    DbSet<CaseStatus> CaseStatuss { get; }
    DbSet<HearingStage> HearingStages { get; }
    DbSet<PredefinedSubject> PredefinedSubjects { get; }
    DbSet<ClientRoleInCase> ClientRolesInCase { get; }
    DbSet<CaseParticipant> CaseParticipants { get; }
    DbSet<CaseDetailsStage> CaseDetailsStages { get; }
    DbSet<Judge> Judges { get; }
    DbSet<CourtType> CourtTypes { get; }
    DbSet<CourtSubtype> CourtSubtypes { get; }
    DbSet<JudicialDecision> JudicialDecisions { get; }
    DbSet<JudicialDeadline> JudicialDeadlines { get; }
    DbSet<JudicialNotice> JudicialNotices { get; }
    DbSet<JudicialAction> JudicialActions { get; }
    DbSet<Fcm> Fcms { get; }
    DbSet<ServiceType> ServiceTypes { get; }
    DbSet<ServiceUnit> ServiceUnits { get; }
    DbSet<ServiceSubject> ServiceSubjects { get; }
    DbSet<CaseServiceDetail> CaseServiceDetails { get; }
    DbSet<PaymentType> PaymentTypes { get; }
    DbSet<PaymentMethodSchedule> PaymentMethodSchedules { get; }
    DbSet<CasePaymentAgreementDetail> CasePayAgreeDetails { get; }
    DbSet<Bank> Banks { get; }
    DbSet<BankBranch> BankBranches { get; }
    DbSet<BankAccount> BankAccounts { get; }
    DbSet<CasePaymentTransactionsDetail> CasePayTnxDetail { get; }
    DbSet<PaymentMethodOption> PaymentMethodOptions { get;}
    DbSet<PayOptionCash> PayOptionCashes { get;}
    DbSet<PayOptionDeposit> PayOptionDeposits { get;}
    DbSet<PayOptionCheck> PayOptionChecks { get;}
    DbSet<PayOptionOther> PayOptionOthers { get;}
    DbSet<FileAccessLog> FileAccessLogs { get; }
    DbSet<EncryptionKey> EncryptionKeys { get; }
    DbSet<EncryptedFileMetadata> EncryptedFileMetadata { get; set; }
    DbSet<FileAccessToken> FileAccessTokens { get; set; }
    DbSet<PreferenceKey> PreferenceKeys { get; }
    DbSet<UserPreference> UserPreferences { get;}
    DbSet<UserDevice> UserDevices { get; }
    DbSet<RolePermission> RolePermissions { get; }

    DbSet<ChatRoom> ChatRooms { get; }
    DbSet<ChatRoomMember> ChatRoomMembers { get; }
    DbSet<ChatMessage> ChatMessages { get; }
    DbSet<MessageStatus> MessageStatuses { get; }
    DbSet<MessageReaction> MessageReactions { get; }
    DbSet<UserConnection> UserConnections { get; }
    DbSet<GuestUser> GuestUsers { get; }
    DbSet<SupportTicket> SupportTickets { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
