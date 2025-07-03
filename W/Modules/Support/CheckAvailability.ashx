<%@ WebHandler Language="C#" Class="CheckAvailability" %>

using System;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Rubik_Support.BLL;

public class CheckAvailability : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        var serializer = new JavaScriptSerializer();
        
        try
        {
            var bll = new SupportBLL();
            var onlineAgents = bll.GetOnlineAgents();
            
            // Check module status
            var isModuleEnabled = bll.IsModuleEnabled();
            
            // Check working hours (optional - can be configured)
            var isInWorkingHours = CheckWorkingHours();
            
            var response = new
            {
                success = true,
                hasOnlineAgents = onlineAgents.Count > 0,
                onlineAgentCount = onlineAgents.Count,
                isModuleEnabled = isModuleEnabled,
                isInWorkingHours = isInWorkingHours,
                availableAgents = onlineAgents.Select(a => new
                {
                    name = a.UserFullName,
                    isAvailable = a.CanHandleMoreTickets,
                    currentLoad = a.CurrentActiveTickets,
                    maxLoad = a.MaxConcurrentTickets
                }),
                estimatedResponseTime = GetEstimatedResponseTime(onlineAgents.Count),
                alternativeContact = GetAlternativeContact()
            };
            
            context.Response.Write(serializer.Serialize(response));
        }
        catch (Exception ex)
        {
            context.Response.Write(serializer.Serialize(new 
            {
                success = false,
                error = "خطا در بررسی وضعیت",
                hasOnlineAgents = false
            }));
        }
    }
    
    private bool CheckWorkingHours()
    {
        var now = DateTime.Now;
        var dayOfWeek = now.DayOfWeek;
        
        // Skip Friday (weekend in Iran)
        if (dayOfWeek == DayOfWeek.Friday)
            return false;
        
        // Check hour (8 AM to 6 PM)
        if (now.Hour < 8 || now.Hour >= 18)
            return false;
        
        return true;
    }
    
    private string GetEstimatedResponseTime(int onlineAgentCount)
    {
        if (onlineAgentCount == 0)
            return "نامشخص";
        else if (onlineAgentCount >= 3)
            return "کمتر از 5 دقیقه";
        else if (onlineAgentCount >= 1)
            return "5 تا 15 دقیقه";
        else
            return "15 تا 30 دقیقه";
    }
    
    private object GetAlternativeContact()
    {
        // Get from settings
        var dal = new Rubik_Support.DAL.SupportDAL();
        var phone = dal.GetSetting("AlternativePhone");
        var email = dal.GetSetting("AlternativeEmail");
        
        if (!string.IsNullOrEmpty(phone) || !string.IsNullOrEmpty(email))
        {
            return new
            {
                phone = phone,
                email = email,
                message = "در صورت نیاز فوری می‌توانید تماس بگیرید"
            };
        }
        
        return null;
    }
    
    public bool IsReusable
    {
        get { return false; }
    }
}