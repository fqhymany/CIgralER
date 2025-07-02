using Rubik_Support.BLL;
using Rubik_Support.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Rubik_Support.Hubs
{
    [HubName("supportHub")]
    public class SupportHub : Hub
    {
        private static readonly Dictionary<string, string> _connections = new Dictionary<string, string>();
        private readonly SupportBLL _bll = new SupportBLL();

        public override Task OnConnected()
        {
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var agentEntry = _connections.FirstOrDefault(x => x.Value == Context.ConnectionId && x.Key.StartsWith("agent-"));
            if (!string.IsNullOrEmpty(agentEntry.Key))
            {
                var userId = Convert.ToInt32(agentEntry.Key.Replace("agent-", ""));
                _bll.SetAgentOnline(userId, false);
                _connections.Remove(agentEntry.Key);
            }

            var ticketId = _connections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            if (!string.IsNullOrEmpty(ticketId))
            {
                _connections.Remove(ticketId);
                Groups.Remove(Context.ConnectionId, $"ticket-{ticketId}");
            }

            return base.OnDisconnected(stopCalled);
        }

        // Visitor joins chat
        public async Task JoinChat(int ticketId)
        {
            _connections[$"ticket-{ticketId}"] = Context.ConnectionId;
            await Groups.Add(Context.ConnectionId, $"ticket-{ticketId}");

            // Notify support users
            await Clients.Group("support").ticketOnline(ticketId);
        }

        // Support joins chat
        public async Task JoinSupport(int userId)
        {
            _bll.SetAgentOnline(userId, true);

            await Groups.Add(Context.ConnectionId, "support");

            _connections[$"agent-{userId}"] = Context.ConnectionId;

            var tickets = _bll.GetActiveTickets(userId);
            await Clients.Caller.loadActiveTickets(tickets);
        }

        // Send message
        public async Task SendMessage(int ticketId, string message, bool isSupport)
        {
            try
            {
                var senderType = isSupport ? SenderType.Support : SenderType.Visitor;
                var messageId = _bll.SendMessage(ticketId, message, null, senderType);

                var messageData = new
                {
                    id = messageId,
                    ticketId = ticketId,
                    message = message,
                    senderType = senderType,
                    createDate = DateTime.Now,
                    senderName = isSupport ? "پشتیبان" : "بازدیدکننده"
                };

                // Send to ticket group
                await Clients.Group($"ticket-{ticketId}").receiveMessage(messageData);

                // Notify support group
                if (!isSupport)
                {
                    await Clients.Group("support").newMessage(ticketId, message);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.error($"خطا در ارسال پیام: {ex.Message}");
            }
        }

        // Support typing indicator
        public async Task Typing(int ticketId, bool isTyping)
        {
            await Clients.OthersInGroup($"ticket-{ticketId}").typing(isTyping);
        }

        // Mark messages as read
        public async Task MarkAsRead(int ticketId, int userId)
        {
            _bll.MarkMessagesAsRead(ticketId, userId);
            await Clients.OthersInGroup($"ticket-{ticketId}").messagesRead(ticketId);
        }

        // Close ticket
        public async Task CloseTicket(int ticketId, int userId)
        {
            _bll.CloseTicket(ticketId, userId);
            await Clients.Group($"ticket-{ticketId}").ticketClosed(ticketId);
            await Clients.Group("support").ticketClosed(ticketId);
        }

        // Assign ticket to support
        public async Task AssignTicket(int ticketId, int supportUserId)
        {
            _bll.AssignTicket(ticketId, supportUserId);
            await Clients.Group("support").ticketAssigned(ticketId, supportUserId);
        }

        public async Task RequestAutoAssign(int ticketId)
        {
            var agent = _bll.AssignTicketToAgent(ticketId);
            if (agent != null)
            {
                await Clients.Group("support").ticketAssigned(ticketId, agent.UserId);
                await Clients.Caller.agentAssigned(agent.UserFullName);
            }
            else
            {
                await Clients.Caller.noAgentAvailable();
            }
        }
    }
}