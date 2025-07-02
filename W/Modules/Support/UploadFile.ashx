<%@ WebHandler Language="C#" Class="UploadFile" %>

using System;
using System.Web;
using System.Web.Script.Serialization;
using Rubik_Support.BLL;
using Rubik_Support.Models;

public class UploadFile : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        var serializer = new JavaScriptSerializer();
        
        try
        {
            var ticketId = Convert.ToInt32(context.Request.Form["ticketId"]);
            var isSupport = context.Request.Form["isSupport"] == "true";
            var file = context.Request.Files["file"];
            
            if (file == null || file.ContentLength == 0)
            {
                context.Response.Write(serializer.Serialize(new 
                {
                    success = false,
                    message = "فایلی انتخاب نشده است"
                }));
                return;
            }
            
            var bll = new SupportBLL();
            var userId = isSupport ? Convert.ToInt32(context.Session["UserId"]) : (int?)null;
            var senderType = isSupport ? SenderType.Support : SenderType.Visitor;
            
            var attachments = new System.Collections.Generic.List<HttpPostedFile> { file };
            var messageId = bll.SendMessage(ticketId, $"فایل پیوست: {file.FileName}", 
                userId, senderType, attachments);
            
            context.Response.Write(serializer.Serialize(new 
            {
                success = true,
                messageId = messageId,
                fileName = file.FileName
            }));
        }
        catch (Exception ex)
        {
            context.Response.Write(serializer.Serialize(new 
            {
                success = false,
                message = "خطا در آپلود فایل: " + ex.Message
            }));
        }
    }
    
    public bool IsReusable
    {
        get { return false; }
    }
}