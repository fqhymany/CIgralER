using Rubik_Support.BLL;
using Rubik_Support.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Collections.Concurrent;

namespace Rubik_Support.Hubs
{
    [HubName("supportHub")]
    public class SupportHub : Hub
    {
        private static readonly ConcurrentDictionary<string, UserConnection> _connections =
            new ConcurrentDictionary<string, UserConnection>();
        private readonly SupportBLL _bll = new SupportBLL();

        public class UserConnection
        {
            public string ConnectionId { get; set; }
            public int? UserId { get; set; }
            public string UserType { get; set; } // "Agent", "User", "Visitor"
            public DateTime ConnectedAt { get; set; }
            public DateTime LastActivity { get; set; }
        }

        public override Task OnConnected()
        {
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            UserConnection connection;
            if (_connections.TryRemove(Context.ConnectionId, out connection))
            {
                if (connection.UserType == "Agent" && connection.UserId.HasValue)
                {
                    _bll.SetAgentOnline(connection.UserId.Value, false);
                    Groups.Remove(Context.ConnectionId, "agents");

                    // Notify other agents
                    Clients.Group("agents").agentStatusChanged(connection.UserId.Value, false);
                }
                else if (connection.UserType == "Visitor" || connection.UserType == "User")
                {
                    // Remove from ticket groups
                    var ticketGroups = GetUserTicketGroups(Context.ConnectionId);
                    foreach (var group in ticketGroups)
                    {
                        Groups.Remove(Context.ConnectionId, group);
                    }
                }
            }

            return base.OnDisconnected(stopCalled);
        }

        // visitor/user join
        public async Task JoinChat(int ticketId, bool isAuthenticated = false)
        {
            var ticket = _bll.GetTicket(ticketId);
            if (ticket == null) return;

            var userType = isAuthenticated ? "User" : "Visitor";
            _connections[Context.ConnectionId] = new UserConnection
            {
                ConnectionId = Context.ConnectionId,
                UserId = ticket.UserId,
                UserType = userType,
                ConnectedAt = DateTime.Now,
                LastActivity = DateTime.Now
            };

            await Groups.Add(Context.ConnectionId, $"ticket-{ticketId}");

            // Load messages
            await Clients.Caller.loadMessages(ticket.Messages.Select(m => new
            {
                id = m.Id,
                message = m.Message,
                senderType = (int)m.SenderType,
                senderName = m.SenderName,
                createDate = m.CreateDate,
                attachments = m.Attachments
            }));

            // Notify agents
            await Clients.Group("agents").ticketOnline(ticketId, userType);
        }

        // agent join
        public async Task JoinSupport(int userId)
        {
            var agent = _bll.GetOrCreateAgent(userId);
            if (agent == null || !agent.IsActive)
            {
                await Clients.Caller.error("شما دسترسی پشتیبانی ندارید");
                return;
            }

            _connections[Context.ConnectionId] = new UserConnection
            {
                ConnectionId = Context.ConnectionId,
                UserId = userId,
                UserType = "Agent",
                ConnectedAt = DateTime.Now,
                LastActivity = DateTime.Now
            };

            _bll.SetAgentOnline(userId, true);
            await Groups.Add(Context.ConnectionId, "agents");

            // Load active tickets
            var tickets = _bll.GetActiveTickets();
            await Clients.Caller.loadDashboard(new
            {
                tickets = tickets.Select(t => new
                {
                    id = t.Id,
                    ticketNumber = t.TicketNumber,
                    subject = t.Subject,
                    status = (int)t.Status,
                    createDate = t.CreateDate,
                    visitor = t.Visitor,
                    supportUserId = t.SupportUserId,
                    supportFullName = t.SupportFullName,
                    isWaitingForAssignment = t.IsWaitingForAssignment,
                    lastMessage = t.Messages?.LastOrDefault()?.Message
                }),
                agentInfo = new
                {
                    currentTickets = agent.CurrentActiveTickets,
                    maxTickets = agent.MaxConcurrentTickets,
                    canHandleMore = agent.CanHandleMoreTickets
                }
            });

            // Notify other agents
            await Clients.OthersInGroup("agents").agentStatusChanged(userId, true);
        }

        // Send message withfeatures
        public async Task SendMessage(int ticketId, string message, bool isSupport)
        {
            try
            {
                var connection = _connections.GetOrAdd(Context.ConnectionId, new UserConnection());
                connection.LastActivity = DateTime.Now;

                var senderId = connection.UserId;
                var senderType = isSupport ? SenderType.Support :
                    (senderId.HasValue ? SenderType.Support : SenderType.Visitor);

                var messageId = _bll.SendMessage(ticketId, message, senderId, senderType);

                var messageData = new
                {
                    id = messageId,
                    ticketId = ticketId,
                    message = message,
                    senderType = senderType,
                    createDate = DateTime.Now,
                    senderName = isSupport ? "پشتیبان" : "کاربر"
                };

                // Send to all in ticket group
                await Clients.Group($"ticket-{ticketId}").receiveMessage(messageData);

                // Update ticket list for agents
                if (!isSupport)
                {
                    await Clients.Group("agents").ticketUpdated(ticketId, new
                    {
                        lastMessage = message,
                        hasNewMessage = true
                    });
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.error($"خطا در ارسال پیام: {ex.Message}");
            }
        }

        // Accept ticket assignment
        public async Task AcceptTicketAssignment(int ticketId)
        {
            try
            {
                var connection = GetCurrentConnection();
                if (connection?.UserId == null || connection.UserType != "Agent")
                {
                    await Clients.Caller.error("دسترسی غیرمجاز");
                    return;
                }

                _bll.AcceptTicketAssignment(ticketId, connection.UserId.Value);

                // Join ticket group
                await Groups.Add(Context.ConnectionId, $"ticket-{ticketId}");

                // Notify all
                await Clients.All.ticketAssigned(ticketId, connection.UserId.Value);
                await Clients.Caller.assignmentAccepted(ticketId);
            }
            catch (Exception ex)
            {
                await Clients.Caller.error(ex.Message);
            }
        }

        // Decline ticket assignment
        public async Task DeclineTicketAssignment(int ticketId)
        {
            try
            {
                var connection = GetCurrentConnection();
                if (connection?.UserId == null || connection.UserType != "Agent")
                {
                    await Clients.Caller.error("دسترسی غیرمجاز");
                    return;
                }

                _bll.DeclineTicketAssignment(ticketId, connection.UserId.Value);
                await Clients.Caller.assignmentDeclined(ticketId);
            }
            catch (Exception ex)
            {
                await Clients.Caller.error(ex.Message);
            }
        }

        // typing indicator
        public async Task Typing(int ticketId, bool isTyping)
        {
            var connection = GetCurrentConnection();
            if (connection != null)
            {
                connection.LastActivity = DateTime.Now;
                await Clients.OthersInGroup($"ticket-{ticketId}").typing(isTyping, connection.UserType);
            }
        }

        // Mark messages as read
        public async Task MarkAsRead(int ticketId)
        {
            var connection = GetCurrentConnection();
            if (connection?.UserId != null)
            {
                _bll.MarkMessagesAsRead(ticketId, connection.UserId.Value);
                await Clients.OthersInGroup($"ticket-{ticketId}").messagesRead(ticketId);
            }
        }

        // Transfer ticket to another agent
        public async Task TransferTicket(int ticketId, int targetAgentId)
        {
            try
            {
                var connection = GetCurrentConnection();
                if (connection?.UserType != "Agent")
                {
                    await Clients.Caller.error("فقط پشتیبان‌ها می‌توانند تیکت انتقال دهند");
                    return;
                }

                _bll.ReleaseTicketFromAgent(ticketId);
                _bll.AssignTicket(ticketId, targetAgentId);

                await Clients.All.ticketTransferred(ticketId, targetAgentId);
            }
            catch (Exception ex)
            {
                await Clients.Caller.error(ex.Message);
            }
        }

        // Heartbeat to keep connection alive
        public void Heartbeat()
        {
            var connection = GetCurrentConnection();
            if (connection != null)
            {
                connection.LastActivity = DateTime.Now;

                if (connection.UserType == "Agent" && connection.UserId.HasValue)
                {
                    // Update agent online status
                    _bll.SetAgentOnline(connection.UserId.Value, true);
                }
            }
        }

        // Get online agents
        public async Task GetOnlineAgents()
        {
            var agents = _bll.GetOnlineAgents();
            await Clients.Caller.updateOnlineAgents(agents.Select(a => new
            {
                id = a.Id,
                userId = a.UserId,
                name = a.UserFullName,
                currentTickets = a.CurrentActiveTickets,
                maxTickets = a.MaxConcurrentTickets,
                isAvailable = a.IsAvailable
            }));
        }

        // Helper methods
        private UserConnection GetCurrentConnection()
        {
            UserConnection connection;
            _connections.TryGetValue(Context.ConnectionId, out connection);
            return connection;
        }

        private List<string> GetUserTicketGroups(string connectionId)
        {
            // This would need to be tracked separately in production
            return new List<string>();
        }
    }
}