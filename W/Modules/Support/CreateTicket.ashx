<%@ WebHandler Language="C#" Class="CreateTicket" %>

using System;
using System.Web;
using System.Web.Script.Serialization;
using Rubik_Support.BLL;

public class CreateTicket : IHttpHandler, System.Web.SessionState.IRequiresSessionState
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
            
            var subject = data["subject"]?.ToString();
            var initialMessage = data["initialMessage"]?.ToString();
            
            // Optional fields (only for guests)
            var mobile = data["mobile"]?.ToString();
            var firstName = data["firstName"]?.ToString();
            var lastName = data["lastName"]?.ToString();
            
            // Check if user is logged in
            var userId = context.Session["UserId"] as int?;
            
            if (!userId.HasValue || userId.Value <= 0)
            {
                // Guest user - mobile is required
                if (string.IsNullOrEmpty(mobile))
                {
                    context.Response.Write(serializer.Serialize(new 
                    {
                        success = false,
                        message = "برای کاربران مهمان، شماره موبایل الزامی است"
                    }));
                    return;
                }
            }
            
            var bll = new SupportBLL();
            
            // Use the new method that handles user detection and rate limiting
            var ticketId = bll.CreateTicketWithUserDetection(
                context, 
                subject, 
                initialMessage, 
                mobile);
            
            // Get ticket info for response
            var ticket = bll.GetTicket(ticketId);
            
            context.Response.Write(serializer.Serialize(new 
            {
                success = true,
                ticketId = ticketId,
                ticketNumber = ticket.TicketNumber,
                message = "تیکت با موفقیت ایجاد شد",
                isUserLoggedIn = userId.HasValue && userId.Value > 0,
                assignmentStatus = ticket.SupportUserId.HasValue ? 
                    "assigned" : "pending"
            }));
        }
        catch (Exception ex)
        {
            context.Response.Write(serializer.Serialize(new 
            {
                success = false,
                message = ex.Message,
                isRateLimitError = ex.Message.Contains("حد مجاز")
            }));
        }
    }
    
    public bool IsReusable
    {
        get { return false; }
    }
}