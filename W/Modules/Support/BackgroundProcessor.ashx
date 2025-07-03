<%@ WebHandler Language="C#" Class="BackgroundProcessor" %>

using System;
using System.Web;
using Rubik_Support.BLL;

public class BackgroundProcessor : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "text/plain";
        
        try
        {
            // Security check - only allow from localhost or with proper key
            var key = context.Request.QueryString["key"];
            var expectedKey = System.Configuration.ConfigurationManager.AppSettings["BackgroundTaskKey"] ?? "default-key-change-me";
            
            if (key != expectedKey && !IsLocalRequest(context))
            {
                context.Response.StatusCode = 403;
                context.Response.Write("Forbidden");
                return;
            }
            
            var bll = new SupportBLL();
            var task = context.Request.QueryString["task"];
            
            switch (task?.ToLower())
            {
                case "sms":
                    bll.ProcessSMSQueue();
                    context.Response.Write("SMS queue processed");
                    break;
                    
                case "assignments":
                    bll.ProcessExpiredAssignments();
                    context.Response.Write("Expired assignments processed");
                    break;
                    
                case "status":
                    bll.UpdateAgentOnlineStatus();
                    context.Response.Write("Agent status updated");
                    break;
                    
                case "all":
                default:
                    bll.ProcessSMSQueue();
                    bll.ProcessExpiredAssignments();
                    bll.UpdateAgentOnlineStatus();
                    context.Response.Write("All tasks processed");
                    break;
            }
            
            context.Response.StatusCode = 200;
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.Write("Error: " + ex.Message);
        }
    }
    
    private bool IsLocalRequest(HttpContext context)
    {
        if (context.Request.IsLocal)
            return true;
            
        var ip = context.Request.UserHostAddress;
        return ip == "127.0.0.1" || ip == "::1";
    }
    
    public bool IsReusable
    {
        get { return false; }
    }
}
