<%@ WebHandler Language="C#" Class="GetTicket" %>

using System;
using System.Web;
using System.Web.Script.Serialization;
using Rubik_Support.BLL;

public class GetTicket : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        var serializer = new JavaScriptSerializer();

        try
        {
            var ticketId = Convert.ToInt32(context.Request.QueryString["ticketId"]);
            var bll = new SupportBLL();
            var ticket = bll.GetTicket(ticketId);

            if (ticket == null)
            {
                context.Response.Write(serializer.Serialize(new
                {
                    success = false,
                    message = "تیکت یافت نشد"
                }));
                return;
            }

            var ticketData = new
            {
                id = ticket.Id,
                ticketNumber = ticket.TicketNumber,
                subject = ticket.Subject,
                status = (int)ticket.Status,
                createDate = ticket.CreateDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                visitor = ticket.Visitor != null ? new
                {
                    id = ticket.Visitor.Id,
                    mobile = ticket.Visitor.Mobile,
                    fullName = ticket.Visitor.FullName
                } : null,
                supportUserId = ticket.SupportUserId,
                supportFullName = ticket.SupportFullName
            };

            context.Response.Write(serializer.Serialize(new
            {
                success = true,
                ticket = ticketData
            }));
        }
        catch (Exception ex)
        {
            context.Response.Write(serializer.Serialize(new
            {
                success = false,
                message = "خطا در دریافت اطلاعات تیکت: " + ex.Message
            }));
        }
    }

    public bool IsReusable
    {
        get { return false; }
    }
}