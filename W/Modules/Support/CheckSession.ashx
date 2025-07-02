<%@ WebHandler Language="C#" Class="CheckSession" %>

using System;
using System.Web;
using System.Web.Script.Serialization;
using System.Linq;
using Rubik_Support.DAL;
using Rubik_Support.Models;

public class CheckSession : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        var serializer = new JavaScriptSerializer();

        try
        {
            var mobile = context.Request.QueryString["mobile"];
            if (string.IsNullOrEmpty(mobile))
            {
                context.Response.Write(serializer.Serialize(new
                {
                    hasActiveTicket = false
                }));
                return;
            }

            var dal = new SupportDAL();
            var visitor = dal.GetVisitorByMobile(mobile);

            if (visitor == null)
            {
                context.Response.Write(serializer.Serialize(new
                {
                    hasActiveTicket = false
                }));
                return;
            }

            // Get active ticket
            var tickets = dal.GetActiveTickets();
            var activeTicket = tickets.FirstOrDefault(t => t.VisitorId == visitor.Id);

            if (activeTicket != null)
            {
                // Count unread messages
                var messages = dal.GetTicketMessages(activeTicket.Id);
                var unreadCount = messages.Count(m => !m.IsRead &&
                                                     m.SenderType == SenderType.Support);

                context.Response.Write(serializer.Serialize(new
                {
                    hasActiveTicket = true,
                    ticketId = activeTicket.Id,
                    unreadCount = unreadCount
                }));
            }
            else
            {
                context.Response.Write(serializer.Serialize(new
                {
                    hasActiveTicket = false
                }));
            }
        }
        catch (Exception ex)
        {
            context.Response.Write(serializer.Serialize(new
            {
                hasActiveTicket = false,
                error = ex.Message
            }));
        }
    }

    public bool IsReusable
    {
        get { return false; }
    }
}