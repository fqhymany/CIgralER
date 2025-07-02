using Rubik_Support.DAL;
using Rubik_Support.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Kavenegar;
using Rubik_SDK;
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
            var msg = new SupportMessage
            {
                TicketId = ticketId,
                Message = message,
                SenderId = senderId,
                SenderType = senderType
            };

            msg.Id = _dal.CreateMessage(msg);

            // Handle attachments
            if (attachments != null && attachments.Count > 0)
            {
                foreach (var file in attachments)
                {
                    if (IsValidFile(file))
                    {
                        var attachment = SaveAttachment(file, msg.Id);
                        if (attachment != null)
                        {
                            _dal.CreateAttachment(attachment);
                        }
                    }
                }
            }

            // Update ticket last update date
            _dal.UpdateTicketStatus(ticketId, TicketStatus.InProgress);

            // Send notification
            SendMessageNotification(ticketId, senderType);

            return msg.Id;
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

    }
}