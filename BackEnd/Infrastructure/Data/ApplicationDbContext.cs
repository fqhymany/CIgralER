
using System.Reflection;
using System.Security.Cryptography.Xml;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Domain.Entities;
using LawyerProject.Domain.Entities.BankEntities;
using LawyerProject.Domain.Entities.CaseFinancials;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LawyerProject.Infrastructure.Data;
public class ApplicationDbContext : IdentityDbContext<User, Role, string, IdentityUserClaim<string>, UsersRole, IdentityUserLogin<string>, RolePermission, IdentityUserToken<string>>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Case> Cases { get; set; } = null!;

    public DbSet<Notification> Notifications { get; set; } = null!;

    public DbSet<Package> Packages { get; set; } = null!;

    public DbSet<PackageSubscription> PackageSubscriptions { get; set; } = null!;

    public DbSet<Payment> Payments { get; set; } = null!;

    public DbSet<Region> Regions { get; set; } = null!;

    public DbSet<RegionsUser> RegionsUsers { get; set; } = null!;

    public override DbSet<Role> Roles { get; set; } = null!;

    public DbSet<RolePermission> RolePermissions { get; set; } = null!;

    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();
    public DbSet<ChatRoomMember> ChatRoomMembers => Set<ChatRoomMember>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<MessageStatus> MessageStatuses => Set<MessageStatus>();
    public DbSet<MessageReaction> MessageReactions => Set<MessageReaction>();
    public DbSet<UserConnection> UserConnections => Set<UserConnection>();
    public DbSet<GuestUser> GuestUsers => Set<GuestUser>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();


    public DbSet<TicketReply> TicketReplies { get; set; } = null!;

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public DbSet<TodoList> TodoLists => Set<TodoList>();

    public override DbSet<User> Users { get; set; } = null!;

    public DbSet<UsersRole> UsersRoles { get; set; } = null!;

    public DbSet<Wallet> Wallets { get; set; } = null!;

    public DbSet<WalletTransaction> WalletTransactions { get; set; } = null!;

    public DbSet<CaseType> CaseTypes { get; set; } = null!;
    public DbSet<HearingStage> HearingStages { get; set; } = null!;
    public DbSet<ClientRoleInCase> ClientRolesInCase { get; set; } = null!;
    public DbSet<PredefinedSubject> PredefinedSubjects { get; set; } = null!;
    public DbSet<CaseStatus> CaseStatuss { get; set; } = null!;
    public DbSet<CaseParticipant> CaseParticipants { get; set; } = null!;
    public DbSet<CaseDetailsStage> CaseDetailsStages { get; set; } = null!;
    public DbSet<Judge> Judges { get; set; } = null!;
    public DbSet<CourtType> CourtTypes { get; set; } = null!;
    public DbSet<CourtSubtype> CourtSubtypes { get; set; } = null!;
    public DbSet<JudicialDecision> JudicialDecisions { get; set; } = null!;
    public DbSet<JudicialDeadline> JudicialDeadlines { get; set; } = null!;
    public DbSet<JudicialNotice> JudicialNotices { get; set; } = null!;
    public DbSet<JudicialAction> JudicialActions { get; set; } = null!;
    public DbSet<Fcm> Fcms { get; set; } = null!;
    public DbSet<ServiceType> ServiceTypes { get; set; } = null!;
    public DbSet<ServiceUnit> ServiceUnits { get; set; } = null!;
    public DbSet<ServiceSubject> ServiceSubjects { get; set; } = null!;
    public DbSet<CaseServiceDetail> CaseServiceDetails { get; set; } = null!;
    public DbSet<EncryptionKey> EncryptionKeys { get; set; } = null!;
    public DbSet<EncryptedFileMetadata> EncryptedFileMetadata { get; set; }
    public DbSet<FileAccessToken> FileAccessTokens { get; set; }
    public DbSet<FileAccessLog> FileAccessLogs { get; set; } = null!;

    public DbSet<PaymentType> PaymentTypes { get; set; } = null!;
    public DbSet<PaymentMethodSchedule> PaymentMethodSchedules { get; set; } = null!;
    public DbSet<CasePaymentAgreementDetail> CasePayAgreeDetails { get; set; } = null!;
    public DbSet<Bank> Banks { get; set; } = null!;
    public DbSet<BankBranch> BankBranches { get; set; } = null!;
    public DbSet<BankAccount> BankAccounts { get; set; } = null!;
    public DbSet<CasePaymentTransactionsDetail> CasePayTnxDetail { get; set; } = null!;
    public DbSet<PaymentMethodOption> PaymentMethodOptions { get; set; } = null!;
    public DbSet<PayOptionCash> PayOptionCashes { get; set; } = null!;
    public DbSet<PayOptionDeposit> PayOptionDeposits { get; set; } = null!;
    public DbSet<PayOptionCheck> PayOptionChecks { get; set; } = null!;
    public DbSet<PayOptionOther> PayOptionOthers { get; set; } = null!;
    public DbSet<PreferenceKey> PreferenceKeys { get; set; } = null!;
    public DbSet<UserPreference> UserPreferences { get; set; } = null!;

    public DbSet<UserDevice> UserDevices { get; set; } = null!;
    //public DbSet<LawFirmOverview> LawFirmOverviews { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=LawyerProjectDb");

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
