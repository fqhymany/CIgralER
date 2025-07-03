using Kavenegar;
using Microsoft.AspNet.SignalR;
using Rubik_SDK;
using Rubik_Support.DAL;
using Rubik_Support.Hubs;
using Rubik_Support.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using LogLevel = Rubik_Support.Models.LogLevel;
namespace Rubik_Support.BLL
{
    public class SupportBLL
    {
        private readonly SupportDAL _dal;
        private readonly string _uploadPath;
        private readonly KavenegarApi _kavenegarApi;

        public SupportBLL()
        {
            _dal = new SupportDAL();
            _uploadPath = HttpContext.Current.Server.MapPath("~/Uploads/Support/");

            // Initialize Kavenegar
            var apiKey = _dal.GetSetting("KavenegarApiKey");
            if (!string.IsNullOrEmpty(apiKey))
            {
                _kavenegarApi = new KavenegarApi(apiKey);
            }
        }

        #region Visitor Management

        public SupportVisitor GetOrCreateVisitor(string mobile, string firstName = null, string lastName = null)
        {
            var visitor = _dal.GetVisitorByMobile(mobile);
            if (visitor == null)
            {
                visitor = new SupportVisitor
                {
                    Mobile = mobile,
                    FirstName = firstName,
                    LastName = lastName
                };
                visitor.Id = _dal.CreateVisitor(visitor);
            }
            else
            {
                _dal.UpdateVisitorLastVisit(visitor.Id);
            }
            return visitor;
        }

        #endregion

        #region Ticket Management

        public int CreateNewTicket(string mobile, string subject, string initialMessage,
            string firstName = null, string lastName = null, int? userId = null)
        {
            SupportVisitor visitor = null;
            if (!string.IsNullOrEmpty(mobile))
            {
                visitor = GetOrCreateVisitor(mobile, firstName, lastName);
            }

            var ticket = new SupportTicket
            {
                TicketNumber = _dal.GenerateTicketNumber(),
                VisitorId = visitor?.Id,
                UserId = userId,
                Subject = subject ?? "تیکت جدید",
                Status = TicketStatus.Open
            };

            ticket.Id = _dal.CreateTicket(ticket);

            // Add initial message
            if (!string.IsNullOrEmpty(initialMessage))
            {
                var message = new SupportMessage
                {
                    TicketId = ticket.Id,
                    Message = initialMessage,
                    SenderId = userId,
                    SenderType = userId.HasValue ? SenderType.Support : SenderType.Visitor
                };
                _dal.CreateMessage(message);
            }

            // Send SMS notification to support
            SendNewTicketNotification(ticket);

            // Log
            _dal.AddLog(LogLevel.Info, $"تیکت جدید ایجاد شد: {ticket.TicketNumber}", userId, ticket.Id);

            return ticket.Id;
        }

        public SupportTicket GetTicket(int ticketId)
        {
            var ticket = _dal.GetTicketById(ticketId);
            if (ticket != null)
            {
                ticket.Messages = _dal.GetTicketMessages(ticketId);
            }
            return ticket;
        }

        public List<SupportTicket> GetActiveTickets(int? supportUserId = null)
        {
            return _dal.GetActiveTickets(supportUserId);
        }

        public void CloseTicket(int ticketId, int userId)
        {
            _dal.UpdateTicketStatus(ticketId, TicketStatus.Closed, userId);
            _dal.AddLog(LogLevel.Info, $"تیکت بسته شد", userId, ticketId);
        }

        public void AssignTicket(int ticketId, int supportUserId)
        {
            _dal.UpdateTicketStatus(ticketId, TicketStatus.InProgress, supportUserId);
            _dal.AddLog(LogLevel.Info, $"تیکت به پشتیبان اختصاص یافت", supportUserId, ticketId);
        }

        #endregion

        #region Message Management

        public int SendMessage(int ticketId, string message, int? senderId,
            SenderType senderType, List<HttpPostedFile> attachments = null)
        {
            // Cancel any pending SMS for this ticket
            _dal.CancelPendingSMS(ticketId);

            // ثبت پیام متنی
            var supportMessage = new SupportMessage
            {
                TicketId = ticketId,
                Message = message,
                SenderId = senderId,
                SenderType = senderType
            };

            var messageId = _dal.CreateMessage(supportMessage);

            // ذخیره فایل‌های ضمیمه (در صورت وجود)
            if (attachments != null && attachments.Count > 0)
            {
                foreach (var file in attachments)
                {
                    if (IsValidFile(file))
                    {
                        var attachment = SaveAttachment(file, messageId);
                        if (attachment != null)
                        {
                            _dal.CreateAttachment(attachment);
                        }
                    }
                }
            }

            // Schedule delayed notification
            var ticket = _dal.GetTicketById(ticketId);
            if (ticket != null)
            {
                var delayMinutes = Convert.ToInt32(_dal.GetSetting("NotificationDelay") ?? "5");

                if (senderType == SenderType.Support && ticket.Visitor != null)
                {
                    // Schedule notification for visitor
                    ScheduleDelayedNotification(ticketId,
                        $"پاسخ جدید در تیکت {ticket.TicketNumber}",
                        ticket.Visitor.Mobile, delayMinutes);
                }
                else if (senderType == SenderType.Visitor && ticket.SupportUserId.HasValue)
                {
                    // Schedule notification for support
                    var supportMobile = CMSUser.GetUserPhoneById(ticket.SupportUserId.Value);
                    if (!string.IsNullOrEmpty(supportMobile))
                    {
                        ScheduleDelayedNotification(ticketId,
                            $"پیام جدید در تیکت {ticket.TicketNumber}",
                            supportMobile, delayMinutes);
                    }
                }
            }

            return messageId;
        }

        private void ScheduleDelayedNotification(int ticketId, string message,
            string mobile = null, int delayMinutes = 5)
        {
            if (_kavenegarApi == null) return;

            var sms = new SMSQueue
            {
                TicketId = ticketId,
                RecipientMobile = mobile ?? GetSupportManagerMobile(),
                Message = message,
                ScheduledDate = DateTime.Now.AddMinutes(delayMinutes)
            };

            _dal.AddToSMSQueue(sms);
        }

        private string GetSupportManagerMobile()
        {
            // Get mobile of support manager or first available admin
            var supportGroupId = _dal.GetSetting("SupportGroupId");
            if (!string.IsNullOrEmpty(supportGroupId))
            {
                // Get first user in support group
                var agents = _dal.GetAllAgents(isActive: true);
                return agents.FirstOrDefault()?.UserMobile ?? "";
            }
            return "";
        }

        public void EditMessage(int messageId, string newMessage, int userId)
        {
            _dal.UpdateMessage(messageId, newMessage, userId);
            _dal.AddLog(LogLevel.Info, $"پیام ویرایش شد: {messageId}", userId);
        }

        public void DeleteMessage(int messageId, int userId)
        {
            _dal.DeleteMessage(messageId, userId);
            _dal.AddLog(LogLevel.Info, $"پیام حذف شد: {messageId}", userId);
        }

        public void MarkMessagesAsRead(int ticketId, int userId)
        {
            _dal.MarkMessagesAsRead(ticketId, userId);
        }

        #endregion

        #region File Management

        private bool IsValidFile(HttpPostedFile file)
        {
            if (file == null || file.ContentLength == 0)
                return false;

            var maxSize = Convert.ToInt64(_dal.GetSetting("MaxFileSize") ?? "10485760");
            if (file.ContentLength > maxSize)
                return false;

            var allowedTypes = _dal.GetSetting("AllowedFileTypes") ?? ".jpg,.jpeg,.png,.gif,.pdf,.doc,.docx";
            var extension = Path.GetExtension(file.FileName).ToLower();

            return allowedTypes.Split(',').Contains(extension);
        }

        private SupportAttachment SaveAttachment(HttpPostedFile file, int messageId)
        {
            try
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var relativePath = $"Support/{DateTime.Now:yyyy/MM}/";
                var fullPath = Path.Combine(_uploadPath, relativePath);

                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);

                var filePath = Path.Combine(fullPath, fileName);
                file.SaveAs(filePath);

                return new SupportAttachment
                {
                    MessageId = messageId,
                    FileName = file.FileName,
                    FileExtension = Path.GetExtension(file.FileName),
                    FileSize = file.ContentLength,
                    FilePath = relativePath + fileName
                };
            }
            catch (Exception ex)
            {
                _dal.AddLog(LogLevel.Error, $"خطا در ذخیره فایل: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Notifications

        private void SendNewTicketNotification(SupportTicket ticket)
        {
            try
            {
                if (_kavenegarApi == null) return;

                var supportUsers = GetSupportUsers();
                var sender = _dal.GetSetting("KavenegarSender");
                var message = $"تیکت جدید\nشماره: {ticket.TicketNumber}\nموضوع: {ticket.Subject}";

                foreach (var user in supportUsers)
                {
                    if (!string.IsNullOrEmpty(user.Mobile))
                    {
                        _kavenegarApi.Send(sender, user.Mobile, message);
                    }
                }
            }
            catch (Exception ex)
            {
                _dal.AddLog(LogLevel.Error, $"خطا در ارسال پیامک: {ex.Message}");
            }
        }

        private void SendMessageNotification(int ticketId, SenderType senderType)
        {
            try
            {
                if (_kavenegarApi == null) return;

                var ticket = _dal.GetTicketById(ticketId);
                var sender = _dal.GetSetting("KavenegarSender");

                if (senderType == SenderType.Support && ticket.Visitor != null)
                {
                    // Notify visitor
                    var message = $"پاسخ جدید برای تیکت {ticket.TicketNumber} ثبت شد";
                    _kavenegarApi.Send(sender, ticket.Visitor.Mobile, message);
                }
                else if (senderType == SenderType.Visitor && ticket.SupportUserId.HasValue)
                {
                    // Notify support
                    var supportUserMobile = CMSUser.GetUserPhoneById(ticket.SupportUserId.Value);
                    if (supportUserMobile != null && !string.IsNullOrEmpty(supportUserMobile))
                    {
                        var message = $"پیام جدید در تیکت {ticket.TicketNumber}";
                        _kavenegarApi.Send(sender, supportUserMobile, message);
                    }
                }
            }
            catch (Exception ex)
            {
                _dal.AddLog(LogLevel.Error, $"خطا در ارسال پیامک: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        public bool IsModuleEnabled()
        {
            return _dal.GetSetting("ModuleEnabled") == "true";
        }

        #endregion

        #region Agent Management

        public SupportAgent GetOrCreateAgent(int userId)
        {
            var agent = _dal.GetAgentByUserId(userId);
            if (agent == null)
            {
                // Create new agent with default settings
                agent = new SupportAgent
                {
                    UserId = userId,
                    IsActive = true,
                    MaxConcurrentTickets = 5,
                    Priority = 1
                };
                agent.Id = _dal.CreateAgent(agent);
            }
            return agent;
        }

        public List<SupportAgent> GetOnlineAgents()
        {
            return _dal.GetAllAgents(isActive: true, isOnline: true);
        }

        public void SetAgentOnline(int userId, bool isOnline)
        {
            _dal.UpdateAgentOnlineStatus(userId, isOnline);
        }

        public SupportAgent AssignTicketToAgent(int ticketId, string specialty = null)
        {
            var agent = _dal.GetBestAvailableAgent(specialty);
            if (agent != null)
            {
                _dal.UpdateTicketStatus(ticketId, TicketStatus.InProgress, agent.UserId);
                _dal.IncrementAgentTicketCount(agent.Id);
                _dal.LogAgentAction(agent.UserId, AgentActionType.StartChat);
            }
            return agent;
        }

        public void ReleaseTicketFromAgent(int ticketId)
        {
            var ticket = _dal.GetTicketById(ticketId);
            if (ticket?.SupportUserId != null)
            {
                var agent = _dal.GetAgentByUserId(ticket.SupportUserId.Value);
                if (agent != null)
                {
                    _dal.DecrementAgentTicketCount(agent.Id);
                    _dal.LogAgentAction(agent.UserId, AgentActionType.EndChat);
                }
            }
        }

        public bool IsAgentAvailable(int userId)
        {
            var agent = _dal.GetAgentByUserId(userId);
            return agent != null && agent.IsAvailable;
        }

        public void UpdateAgentSettings(SupportAgent agent)
        {
            _dal.UpdateAgent(agent);
        }

        // بازنویسی متد GetSupportUsers
        private List<dynamic> GetSupportUsers()
        {
            var agents = _dal.GetAllAgents(isActive: true);
            return agents.Select(a => new
            {
                a.UserId,
                a.UserFullName,
                Mobile = a.UserMobile,
                a.UserEmail
            }).Cast<dynamic>().ToList();
        }

        // بازنویسی متد HasSupportAccess
        public bool HasSupportAccess(int userId)
        {
            var agent = _dal.GetAgentByUserId(userId);
            return agent != null && agent.IsActive;
        }

        #endregion

        #region Ticket Creation with User Detection

        public int CreateTicketWithUserDetection(HttpContext context, string subject,
            string initialMessage, string mobileOverride = null)
        {
            try
            {
                // 1. Check if user is logged in
                var userId = context.Session["UserId"] as int?;
                string mobile = mobileOverride;
                string firstName = null;
                string lastName = null;

                if (userId.HasValue && userId.Value > 0)
                {
                    // Get user info from database
                    var userInfo = CMSUser.GetUserById(userId.Value);
                    var user = userInfo as CMSUser;
                    if (user != null)
                    {
                        mobile = string.IsNullOrEmpty(mobileOverride) ?
                            CMSUser.GetUserPhoneById(userId.Value) : mobileOverride;
                        firstName = user.FirstName;
                        lastName = user.LastName;
                    }
                }

                // 2. Check rate limiting
                var identifier = userId.HasValue ? userId.Value.ToString() : mobile;
                var identifierType = userId.HasValue ? "UserId" : "Mobile";

                if (!CheckRateLimit(identifier, identifierType))
                {
                    throw new Exception("شما به حد مجاز ارسال تیکت رسیده‌اید. لطفا بعداً تلاش کنید.");
                }

                // 3. Create ticket
                var ticketId = CreateNewTicket(mobile, subject, initialMessage,
                    firstName, lastName, userId);

                // 4. Update rate limit
                _dal.UpdateUserLimit(identifier, identifierType);

                // 5. Try auto-assignment
                Task.Run(() => TryAutoAssignTicket(ticketId));

                return ticketId;
            }
            catch (Exception ex)
            {
                _dal.AddLog(LogLevel.Error, $"خطا در ایجاد تیکت: {ex.Message}");
                throw;
            }
        }

        private bool CheckRateLimit(string identifier, string identifierType)
        {
            var limit = _dal.GetUserLimit(identifier, identifierType);
            if (limit == null) return true;

            // Check if blocked
            if (limit.IsBlocked && limit.BlockedUntil.HasValue &&
                limit.BlockedUntil.Value > DateTime.Now)
            {
                return false;
            }

            // Check hourly limit (5 tickets per hour)
            if (limit.TicketCount >= 5 &&
                (DateTime.Now - limit.LastTicketDate).TotalHours < 1)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Smart Ticket Assignment

        private async Task<TicketAssignmentResult> TryAutoAssignTicket(int ticketId,
            int maxAttempts = 3)
        {
            var result = new TicketAssignmentResult { Success = false };

            try
            {
                // Lock ticket for assignment
                if (!_dal.TryLockTicketForAssignment(ticketId))
                {
                    result.FailureReason = AssignmentFailureReason.MaxAttemptsReached;
                    result.Message = "تیکت در حال اختصاص به پشتیبان دیگر است";
                    return result;
                }

                var ticket = _dal.GetTicketById(ticketId);
                var attemptedAgents = new List<int>();

                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    // Get next available agent
                    var agent = _dal.GetNextAvailableAgent(
                        excludeAgentId: attemptedAgents.LastOrDefault(),
                        specialty: ticket.Subject);

                    if (agent == null)
                    {
                        if (attempt == 0)
                        {
                            result.FailureReason = AssignmentFailureReason.NoAgentsAvailable;
                            result.Message = "هیچ پشتیبان آنلاینی موجود نیست";
                        }
                        else
                        {
                            result.FailureReason = AssignmentFailureReason.AllAgentsBusy;
                            result.Message = "تمام پشتیبان‌ها مشغول هستند";
                        }
                        break;
                    }

                    attemptedAgents.Add(agent.Id);

                    // Create assignment request
                    var requestId = _dal.CreateAgentRequest(ticketId, agent.Id,
                        agent.ResponseTimeout);

                    // Send notification to agent
                    await NotifyAgentForAssignment(agent, ticket);

                    // Wait for response
                    var accepted = await WaitForAgentResponse(requestId, agent.ResponseTimeout);

                    if (accepted)
                    {
                        // Assign ticket
                        _dal.UpdateTicketStatus(ticketId, TicketStatus.InProgress, agent.UserId);
                        _dal.IncrementAgentTicketCount(agent.Id);

                        result.Success = true;
                        result.AssignedAgent = agent;
                        result.Message = $"تیکت به {agent.UserFullName} اختصاص یافت";

                        // Cancel pending SMS notifications
                        _dal.CancelPendingSMS(ticketId);

                        return result;
                    }
                    else
                    {
                        // Agent declined or timeout
                        _dal.AddLog(LogLevel.Info,
                            $"Agent {agent.UserId} declined or timeout for ticket {ticketId}");
                    }
                }

                // If no agent accepted, schedule SMS notification
                ScheduleDelayedNotification(ticketId, "تیکت جدید در انتظار پاسخ");

                return result;
            }
            catch (Exception ex)
            {
                _dal.AddLog(LogLevel.Error, $"خطا در اختصاص خودکار: {ex.Message}");
                result.Message = "خطا در اختصاص تیکت";
                return result;
            }
        }

        private async Task NotifyAgentForAssignment(SupportAgent agent, SupportTicket ticket)
        {
            // Send real-time notification via SignalR
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<SupportHub>();
            await hubContext.Clients.User(agent.UserId.ToString())
                .newTicketAssignmentRequest(new
                {
                    ticketId = ticket.Id,
                    ticketNumber = ticket.TicketNumber,
                    subject = ticket.Subject,
                    timeoutSeconds = agent.ResponseTimeout
                });

            // Also send SMS if configured
            if (!string.IsNullOrEmpty(agent.UserMobile) && _kavenegarApi != null)
            {
                try
                {
                    var sender = _dal.GetSetting("KavenegarSender");
                    _kavenegarApi.Send(sender, agent.UserMobile,
                        $"تیکت جدید: {ticket.TicketNumber}\nبرای قبول به پنل مراجعه کنید");
                }
                catch { }
            }
        }

        private async Task<bool> WaitForAgentResponse(int requestId, int timeoutSeconds)
        {
            var endTime = DateTime.Now.AddSeconds(timeoutSeconds);

            while (DateTime.Now < endTime)
            {
                var request = _dal.GetPendingAgentRequest(requestId, 0);
                if (request == null) // Request processed
                {
                    var processed = _dal.GetAgentRequestById(requestId);
                    return processed?.IsAccepted ?? false;
                }

                await Task.Delay(1000); // Check every second
            }

            return false; // Timeout
        }

        #endregion

        #region Enhanced Message Management

        #endregion

        #region Background Tasks

        public void ProcessSMSQueue()
        {
            try
            {
                var pendingSMS = _dal.GetPendingSMS();
                var sender = _dal.GetSetting("KavenegarSender");

                foreach (var sms in pendingSMS)
                {
                    try
                    {
                        // Check if there's new activity on the ticket
                        var ticket = _dal.GetTicketById(sms.TicketId);
                        var messages = _dal.GetTicketMessages(sms.TicketId);
                        var lastMessage = messages.OrderByDescending(m => m.CreateDate).FirstOrDefault();

                        // If there's a new message after SMS was scheduled, cancel it
                        if (lastMessage != null && lastMessage.CreateDate > sms.ScheduledDate.AddMinutes(-5))
                        {
                            _dal.CancelPendingSMS(sms.TicketId);
                            continue;
                        }

                        // Send SMS
                        _kavenegarApi.Send(sender, sms.RecipientMobile, sms.Message);
                        _dal.MarkSMSAsSent(sms.Id);
                    }
                    catch (Exception ex)
                    {
                        _dal.IncrementSMSRetryCount(sms.Id);
                        _dal.AddLog(LogLevel.Error, $"خطا در ارسال SMS {sms.Id}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _dal.AddLog(LogLevel.Error, $"خطا در پردازش صف SMS: {ex.Message}");
            }
        }

        public void ProcessExpiredAssignments()
        {
            try
            {
                var expiredRequests = _dal.GetExpiredRequests();

                foreach (var request in expiredRequests)
                {
                    // Mark as timeout
                    _dal.UpdateAgentRequestResponse(request.Id, false);

                    // Try next agent
                    Task.Run(() => TryAutoAssignTicket(request.TicketId));
                }
            }
            catch (Exception ex)
            {
                _dal.AddLog(LogLevel.Error, $"خطا در پردازش درخواست‌های منقضی: {ex.Message}");
            }
        }

        public void UpdateAgentOnlineStatus()
        {
            try
            {
                var agents = _dal.GetAllAgents(isActive: true, isOnline: true);

                foreach (var agent in agents)
                {
                    // Check last activity
                    if (agent.LastOnlineDate.HasValue &&
                        (DateTime.Now - agent.LastOnlineDate.Value).TotalMinutes > 15)
                    {
                        _dal.UpdateAgentOnlineStatus(agent.UserId, false);
                        _dal.AddLog(LogLevel.Info,
                            $"Agent {agent.UserId} marked offline due to inactivity");
                    }
                }
            }
            catch (Exception ex)
            {
                _dal.AddLog(LogLevel.Error, $"خطا در به‌روزرسانی وضعیت آنلاین: {ex.Message}");
            }
        }

        #endregion

        #region Agent Response Handling

        public void AcceptTicketAssignment(int ticketId, int userId)
        {
            var agent = _dal.GetAgentByUserId(userId);
            if (agent == null) throw new Exception("Agent not found");

            var request = _dal.GetPendingAgentRequest(ticketId, agent.Id);
            if (request == null) throw new Exception("No pending request found");

            // Update request
            _dal.UpdateAgentRequestResponse(request.Id, true);

            // Assign ticket
            _dal.UpdateTicketStatus(ticketId, TicketStatus.InProgress, userId);
            _dal.IncrementAgentTicketCount(agent.Id);

            // Cancel other pending requests for this ticket
            _dal.CancelOtherAgentRequests(ticketId, agent.Id);

            // Notify via SignalR
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<SupportHub>();
            hubContext.Clients.All.ticketAssigned(ticketId, userId);
        }

        public void DeclineTicketAssignment(int ticketId, int userId)
        {
            var agent = _dal.GetAgentByUserId(userId);
            if (agent == null) return;

            var request = _dal.GetPendingAgentRequest(ticketId, agent.Id);
            if (request == null) return;

            // Update request
            _dal.UpdateAgentRequestResponse(request.Id, false);

            // Try next agent
            Task.Run(() => TryAutoAssignTicket(ticketId));
        }

        #endregion
    }
}