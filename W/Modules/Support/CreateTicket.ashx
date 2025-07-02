<%@ WebHandler Language="C#" Class="CreateTicket" %>

using System;
using System.Web;
using System.Web.Script.Serialization;
using Rubik_Support.BLL;

public class CreateTicket : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        var serializer = new JavaScriptSerializer();
        
        try
        {
            // Read JSON data
            var jsonString = new System.IO.StreamReader(context.Request.InputStream).ReadToEnd();
            dynamic data = serializer.DeserializeObject(jsonString);
            
            var mobile = data["mobile"]?.ToString();
            var firstName = data["firstName"]?.ToString();
            var lastName = data["lastName"]?.ToString();
            var subject = data["subject"]?.ToString();
            var initialMessage = data["initialMessage"]?.ToString();
            
            if (string.IsNullOrEmpty(mobile))
            {
                context.Response.Write(serializer.Serialize(new 
                {
                    success = false,
                    message = "شماره موبایل الزامی است"
                }));
                return;
            }
            
            var bll = new SupportBLL();
            var ticketId = bll.CreateNewTicket(mobile, subject, initialMessage, 
                firstName, lastName);
            
            context.Response.Write(serializer.Serialize(new 
            {
                success = true,
                ticketId = ticketId,
                message = "تیکت با موفقیت ایجاد شد"
            }));
        }
        catch (Exception ex)
        {
            context.Response.Write(serializer.Serialize(new 
            {
                success = false,
                message = "خطا در ایجاد تیکت: " + ex.Message
            }));
        }
    }
    
    public bool IsReusable
    {
        get { return false; }
    }
}