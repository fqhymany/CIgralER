<%@ WebHandler Language="C#" Class="GetMessages" %>

using System;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Rubik_Support.BLL;

public class GetMessages : IHttpHandler
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
            
            var messages = ticket.Messages.Select(m => new 
            {
                id = m.Id,
                message = m.Message,
                senderType = (int)m.SenderType,
                senderName = m.SenderName,
                createDate = m.CreateDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                attachments = m.Attachments?.Select(a => new 
                {
                    fileName = a.FileName,
                    filePath = a.FilePath
                })
            });
            
            context.Response.Write(serializer.Serialize(new 
            {
                success = true,
                messages = messages
            }));
        }
        catch (Exception ex)
        {
            context.Response.Write(serializer.Serialize(new 
            {
                success = false,
                message = "خطا در دریافت پیام‌ها: " + ex.Message
            }));
        }
    }
    
    public bool IsReusable
    {
        get { return false; }
    }
}