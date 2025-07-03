<%@ WebHandler Language="C#" Class="CheckVisitor" %>

using System;
using System.Web;
using System.Web.Script.Serialization;
using Rubik_Support.DAL;

public class CheckVisitor : IHttpHandler
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
                    exists = false,
                    error = "شماره موبایل الزامی است"
                }));
                return;
            }
            
            var dal = new SupportDAL();
            var visitor = dal.GetVisitorByMobile(mobile);
            
            if (visitor != null)
            {
                context.Response.Write(serializer.Serialize(new 
                {
                    exists = true,
                    visitor = new
                    {
                        firstName = visitor.FirstName,
                        lastName = visitor.LastName,
                        email = visitor.Email,
                        isBlocked = visitor.IsBlocked
                    }
                }));
            }
            else
            {
                context.Response.Write(serializer.Serialize(new 
                {
                    exists = false
                }));
            }
        }
        catch (Exception ex)
        {
            context.Response.Write(serializer.Serialize(new 
            {
                exists = false,
                error = "خطا در بررسی اطلاعات"
            }));
        }
    }
    
    public bool IsReusable
    {
        get { return false; }
    }
}