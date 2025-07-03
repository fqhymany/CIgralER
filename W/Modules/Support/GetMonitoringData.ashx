<%@ WebHandler Language="C#" Class="GetMonitoringData" %>

using System;
using System.Web;
using System.Web.Script.Serialization;
using System.Data.SqlClient;
using Rubik_SDK;

public class GetMonitoringData : IHttpHandler, System.Web.SessionState.IRequiresSessionState
{
    private string _connectionString = ConnectionEncryption.Decrypt(GlobalDef.ConnectionString);

    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        var serializer = new JavaScriptSerializer();

        // Check admin access
        var userId = context.Session["UserId"] as int?;
        if (!userId.HasValue || !HasAdminAccess(userId.Value))
        {
            context.Response.StatusCode = 403;
            context.Response.Write(serializer.Serialize(new { error = "Access denied" }));
            return;
        }

        try
        {
            var data = new
            {
                pendingSMS = GetPendingSMSCount(),
                expiredRequests = GetExpiredRequestsCount(),
                onlineAgents = GetOnlineAgentsCount(),
                activeTickets = GetActiveTicketsCount(),
                avgResponseTime = GetAverageResponseTime(),
                ticketsPerHour = GetTicketsPerHour(),
                systemHealth = GetSystemHealth()
            };

            context.Response.Write(serializer.Serialize(data));
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.Write(serializer.Serialize(new
            {
                error = "Error loading monitoring data",
                message = ex.Message
            }));
        }
    }

    private bool HasAdminAccess(int userId)
    {
        // Check if user is in support group or is admin
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new SqlCommand(@"
                SELECT COUNT(*) FROM KCI_AssignedUsers au
                INNER JOIN KCI_Groups g ON au.GroupId = g.id
                WHERE au.UserId = @UserId AND g.Name IN ('Administrators', 'Support')", conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
    }

    private int GetPendingSMSCount()
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Support_SMSQueue WHERE IsSent = 0 AND IsCancelled = 0", conn))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }

    private int GetExpiredRequestsCount()
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new SqlCommand(@"
                SELECT COUNT(*) FROM Support_AgentRequests 
                WHERE ResponseDate IS NULL AND TimeoutDate <= GETDATE()", conn))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }

    private int GetOnlineAgentsCount()
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Support_Agents WHERE IsOnline = 1", conn))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }

    private int GetActiveTicketsCount()
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Support_Tickets WHERE Status != 3 AND IsActive = 1", conn))
            {
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }

    private double GetAverageResponseTime()
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new SqlCommand(@"
                SELECT AVG(DATEDIFF(MINUTE, t.CreateDate, m.CreateDate))
                FROM Support_Tickets t
                INNER JOIN (
                    SELECT TicketId, MIN(CreateDate) as CreateDate
                    FROM Support_Messages
                    WHERE SenderType = 2
                    GROUP BY TicketId
                ) m ON t.Id = m.TicketId
                WHERE t.CreateDate >= DATEADD(DAY, -7, GETDATE())", conn))
            {
                var result = cmd.ExecuteScalar();
                return result == DBNull.Value ? 0 : Convert.ToDouble(result);
            }
        }
    }

    private object GetTicketsPerHour()
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new SqlCommand(@"
                SELECT DATEPART(HOUR, CreateDate) as Hour, COUNT(*) as Count
                FROM Support_Tickets
                WHERE CreateDate >= DATEADD(DAY, -1, GETDATE())
                GROUP BY DATEPART(HOUR, CreateDate)
                ORDER BY Hour", conn))
            {
                var hours = new int[24];
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var hour = Convert.ToInt32(reader["Hour"]);
                        var count = Convert.ToInt32(reader["Count"]);
                        hours[hour] = count;
                    }
                }
                return hours;
            }
        }
    }

    private object GetSystemHealth()
    {
        var health = new SystemHealth();

        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();

            // Check for old unprocessed SMS
            using (var cmd = new SqlCommand(@"
            SELECT COUNT(*) FROM Support_SMSQueue 
            WHERE IsSent = 0 AND IsCancelled = 0 
            AND ScheduledDate < DATEADD(HOUR, -1, GETDATE())", conn))
            {
                if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                {
                    health.issues.Add("پیامک‌های پردازش نشده قدیمی وجود دارد");
                }
            }

            // Check for tickets without assignment
            using (var cmd = new SqlCommand(@"
            SELECT COUNT(*) FROM Support_Tickets 
            WHERE Status = 1 AND SupportUserId IS NULL 
            AND CreateDate < DATEADD(HOUR, -2, GETDATE())", conn))
            {
                if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                {
                    health.issues.Add("تیکت‌های بدون پشتیبان قدیمی وجود دارد");
                }
            }
        }

        if (health.issues.Count > 0)
        {
            health.status = "warning";
        }

        return health;
    }

    public bool IsReusable
    {
        get { return false; }
    }
}

public class SystemHealth
{
    public string status { get; set; }
    public System.Collections.Generic.List<string> issues { get; set; }

    public SystemHealth()
    {
        status = "healthy";
        issues = new System.Collections.Generic.List<string>();
    }
}