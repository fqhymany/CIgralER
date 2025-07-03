using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rubik_SDK;
using Rubik_Support.DAL;

public partial class Modules_Support_Admin : System.Web.UI.Page
{

    private SupportDAL _dal = new SupportDAL();
    private string _connectionString = ConnectionEncryption.Decrypt(GlobalDef.ConnectionString);

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack) return;
        LoadSettings();
        LoadGroups();
        LoadSupportUsers();
        LoadStatistics();
        LoadAgents();
        LoadMonitoringData();
        LoadAvailableUsers();
    }

    private void LoadSettings()
    {
        var settings = _dal.GetAllSettings();

        chkModuleEnabled.Checked = settings.ContainsKey("ModuleEnabled") &&
                                   settings["ModuleEnabled"] == "true";
        txtKavenegarApiKey.Text = settings.ContainsKey("KavenegarApiKey") ?
            settings["KavenegarApiKey"] : "";
        txtKavenegarSender.Text = settings.ContainsKey("KavenegarSender") ?
            settings["KavenegarSender"] : "";

        if (settings.ContainsKey("MaxFileSize"))
        {
            var bytes = Convert.ToInt64(settings["MaxFileSize"]);
            txtMaxFileSize.Text = (bytes / 1048576).ToString(); // Convert to MB
        }

        txtAllowedFileTypes.Text = settings.ContainsKey("AllowedFileTypes") ?
            settings["AllowedFileTypes"] : "";

        txtNotificationDelay.Text = settings.ContainsKey("NotificationDelay") ?
            settings["NotificationDelay"] : "5";
        txtMaxTicketsPerHour.Text = settings.ContainsKey("MaxTicketsPerHour") ?
            settings["MaxTicketsPerHour"] : "5";
        txtAlternativePhone.Text = settings.ContainsKey("AlternativePhone") ?
            settings["AlternativePhone"] : "";
        txtAlternativeEmail.Text = settings.ContainsKey("AlternativeEmail") ?
            settings["AlternativeEmail"] : "";
        chkAutoAssignment.Checked = settings.ContainsKey("AutoAssignment") ?
            settings["AutoAssignment"] == "true" : true;
        txtAgentResponseTimeout.Text = settings.ContainsKey("AgentResponseTimeout") ?
            settings["AgentResponseTimeout"] : "60";
        txtMaxAssignmentAttempts.Text = settings.ContainsKey("MaxAssignmentAttempts") ?
            settings["MaxAssignmentAttempts"] : "3";
    }

    private void LoadGroups()
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new SqlCommand("SELECT id, Name FROM KCI_Groups ORDER BY Name", conn))
            {
                ddlSupportGroup.DataSource = cmd.ExecuteReader();
                ddlSupportGroup.DataBind();
                ddlSupportGroup.Items.Insert(0, new ListItem("-- انتخاب کنید --", ""));
            }
        }

        // Select current support group
        var supportGroupId = _dal.GetSetting("SupportGroupId");
        if (!string.IsNullOrEmpty(supportGroupId))
        {
            ddlSupportGroup.SelectedValue = supportGroupId;
        }
    }

    private void LoadSupportUsers()
    {
        var supportGroupId = _dal.GetSetting("SupportGroupId");
        if (string.IsNullOrEmpty(supportGroupId)) return;

        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            var query = @"
                    SELECT DISTINCT u.UserName, 
                           u.FirstName + ' ' + u.LastName AS FullName,
                           u.Mobile, u.Email
                    FROM KCI_Users u
                    INNER JOIN KCI_AssignedUsers au ON u.id = au.UserId
                    WHERE au.GroupId = @GroupId AND u.Enable = 1
                    ORDER BY FullName";

            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@GroupId", supportGroupId);
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    gvSupportUsers.DataSource = dt;
                    gvSupportUsers.DataBind();
                }
            }
        }
    }

    private void LoadStatistics()
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();

            // Total tickets
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Support_Tickets", conn))
            {
                lblTotalTickets.Text = cmd.ExecuteScalar().ToString();
            }

            // Open tickets
            using (var cmd = new SqlCommand(
                       "SELECT COUNT(*) FROM Support_Tickets WHERE Status != 3", conn))
            {
                lblOpenTickets.Text = cmd.ExecuteScalar().ToString();
            }

            // Closed today
            using (var cmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM Support_Tickets 
                    WHERE Status = 3 AND CAST(CloseDate AS DATE) = CAST(GETDATE() AS DATE)", conn))
            {
                lblClosedToday.Text = cmd.ExecuteScalar().ToString();
            }

            // Active supports
            using (var cmd = new SqlCommand(@"
                    SELECT COUNT(DISTINCT SupportUserId) 
                    FROM Support_Tickets 
                    WHERE SupportUserId IS NOT NULL AND Status != 3", conn))
            {
                lblActiveSupports.Text = cmd.ExecuteScalar().ToString();
            }

            // Support performance
            LoadSupportPerformance();
        }
    }

    private void LoadSupportPerformance()
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            var query = @"
                    SELECT u.FirstName + ' ' + u.LastName AS SupportName,
                           COUNT(DISTINCT t.Id) AS TotalTickets,
                           COUNT(DISTINCT CASE WHEN t.Status = 3 THEN t.Id END) AS ClosedTickets,
                           'N/A' AS AvgResponseTime,
                           'N/A' AS Rating
                    FROM KCI_Users u
                    INNER JOIN Support_Tickets t ON u.id = t.SupportUserId
                    GROUP BY u.FirstName, u.LastName
                    ORDER BY TotalTickets DESC";

            using (var cmd = new SqlCommand(query, conn))
            {
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    gvSupportPerformance.DataSource = dt;
                    gvSupportPerformance.DataBind();
                }
            }
        }
    }

    protected void btnSaveSettings_Click(object sender, EventArgs e)
    {
        try
        {
            _dal.UpdateSetting("ModuleEnabled", chkModuleEnabled.Checked ? "true" : "false");
            _dal.UpdateSetting("KavenegarApiKey", txtKavenegarApiKey.Text);
            _dal.UpdateSetting("KavenegarSender", txtKavenegarSender.Text);

            if (!string.IsNullOrEmpty(txtMaxFileSize.Text))
            {
                var mb = Convert.ToInt64(txtMaxFileSize.Text);
                _dal.UpdateSetting("MaxFileSize", (mb * 1048576).ToString());
            }

            _dal.UpdateSetting("AllowedFileTypes", txtAllowedFileTypes.Text);

            _dal.UpdateSetting("NotificationDelay", txtNotificationDelay.Text);
            _dal.UpdateSetting("MaxTicketsPerHour", txtMaxTicketsPerHour.Text);
            _dal.UpdateSetting("AlternativePhone", txtAlternativePhone.Text);
            _dal.UpdateSetting("AlternativeEmail", txtAlternativeEmail.Text);
            _dal.UpdateSetting("AutoAssignment", chkAutoAssignment.Checked ? "true" : "false");
            _dal.UpdateSetting("AgentResponseTimeout", txtAgentResponseTimeout.Text);
            _dal.UpdateSetting("MaxAssignmentAttempts", txtMaxAssignmentAttempts.Text);

            ScriptManager.RegisterStartupScript(this, GetType(), "success",
                "alert('تنظیمات با موفقیت ذخیره شد');", true);
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "error",
                "alert('خطا در ذخیره تنظیمات: " + ex.Message + "');", true);
        }
    }

    protected void btnSaveSupportGroup_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(ddlSupportGroup.SelectedValue))
        {
            _dal.UpdateSetting("SupportGroupId", ddlSupportGroup.SelectedValue);
            LoadSupportUsers();

            ScriptManager.RegisterStartupScript(this, GetType(), "success",
                "alert('گروه پشتیبان‌ها ذخیره شد');", true);
        }
    }

    protected void btnSearchTickets_Click(object sender, EventArgs e)
    {
        SearchTickets();
    }

    private void SearchTickets()
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            var query = @"
                    SELECT t.Id, t.TicketNumber, 
                           ISNULL(v.Mobile, 'N/A') AS VisitorMobile,
                           t.Subject, t.CreateDate,
                           CASE t.Status 
                               WHEN 1 THEN 'باز'
                               WHEN 2 THEN 'در حال بررسی'
                               WHEN 3 THEN 'بسته شده'
                           END AS StatusText
                    FROM Support_Tickets t
                    LEFT JOIN Support_Visitors v ON t.VisitorId = v.Id
                    WHERE 1=1";

            if (!string.IsNullOrEmpty(txtSearchTicketNumber.Text))
            {
                query += " AND t.TicketNumber LIKE @TicketNumber";
            }

            if (!string.IsNullOrEmpty(txtSearchMobile.Text))
            {
                query += " AND v.Mobile LIKE @Mobile";
            }

            if (!string.IsNullOrEmpty(ddlSearchStatus.SelectedValue))
            {
                query += " AND t.Status = @Status";
            }

            query += " ORDER BY t.CreateDate DESC";

            using (var cmd = new SqlCommand(query, conn))
            {
                if (!string.IsNullOrEmpty(txtSearchTicketNumber.Text))
                {
                    cmd.Parameters.AddWithValue("@TicketNumber",
                        "%" + txtSearchTicketNumber.Text + "%");
                }

                if (!string.IsNullOrEmpty(txtSearchMobile.Text))
                {
                    cmd.Parameters.AddWithValue("@Mobile",
                        "%" + txtSearchMobile.Text + "%");
                }

                if (!string.IsNullOrEmpty(ddlSearchStatus.SelectedValue))
                {
                    cmd.Parameters.AddWithValue("@Status",
                        Convert.ToInt32(ddlSearchStatus.SelectedValue));
                }

                using (var adapter = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    gvTickets.DataSource = dt;
                    gvTickets.DataBind();
                }
            }
        }
    }

    protected void gvTickets_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        var ticketId = Convert.ToInt32(e.CommandArgument);

        if (e.CommandName == "ViewTicket")
        {
            Response.Redirect("ViewTicket.aspx?id=" + ticketId);
        }
        else if (e.CommandName == "DeleteTicket")
        {
            DeleteTicket(ticketId);
            SearchTickets();
        }
    }

    private void DeleteTicket(int ticketId)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new SqlCommand(
                       "UPDATE Support_Tickets SET IsActive = 0 WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", ticketId);
                cmd.ExecuteNonQuery();
            }
        }
    }

    private void LoadAgents()
    {
        var agents = _dal.GetAllAgents();
        gvAgents.DataSource = agents;
        gvAgents.DataBind();
    }

    private void LoadAvailableUsers()
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            var query = @"
            SELECT u.id, u.FirstName + ' ' + u.LastName AS FullName
            FROM KCI_Users u
            LEFT JOIN Support_Agents a ON u.id = a.UserId
            WHERE u.Enable = 1 AND a.Id IS NULL
            ORDER BY FullName";

            using (var cmd = new SqlCommand(query, conn))
            {
                ddlNewAgent.DataSource = cmd.ExecuteReader();
                ddlNewAgent.DataBind();
                ddlNewAgent.Items.Insert(0, new ListItem("-- انتخاب کاربر --", ""));
            }
        }
    }

    protected void btnAddAgent_Click(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(ddlNewAgent.SelectedValue))
            {
                ShowMessage("لطفا یک کاربر انتخاب کنید", "error");
                return;
            }

            var agent = new Rubik_Support.Models.SupportAgent
            {
                UserId = Convert.ToInt32(ddlNewAgent.SelectedValue),
                IsActive = true,
                MaxConcurrentTickets = Convert.ToInt32(txtNewAgentMaxTickets.Text),
                Specialties = txtNewAgentSpecialties.Text,
                Priority = 1
            };

            _dal.CreateAgent(agent);

            LoadAgents();
            LoadAvailableUsers();
            ShowMessage("پشتیبان با موفقیت اضافه شد", "success");
        }
        catch (Exception ex)
        {
            ShowMessage("خطا: " + ex.Message, "error");
        }
    }

    protected void gvAgents_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        var agentId = Convert.ToInt32(e.CommandArgument);

        if (e.CommandName == "EditAgent")
        {
            // Show edit modal or redirect to edit page
            Response.Redirect("EditAgent.aspx?id=" + agentId);
        }
        else if (e.CommandName == "ToggleAgent")
        {
            var agent = _dal.GetAgentById(agentId);
            if (agent != null)
            {
                agent.IsActive = !agent.IsActive;
                _dal.UpdateAgent(agent);
                LoadAgents();
            }
        }
    }

    // مانیتورینگ
    private void LoadMonitoringData()
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();

            // Pending SMS
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Support_SMSQueue WHERE IsSent = 0 AND IsCancelled = 0", conn))
            {
                lblPendingSMS.Text = cmd.ExecuteScalar().ToString();
            }

            // Expired Requests
            using (var cmd = new SqlCommand(@"
            SELECT COUNT(*) FROM Support_AgentRequests 
            WHERE ResponseDate IS NULL AND TimeoutDate <= GETDATE()", conn))
            {
                lblExpiredRequests.Text = cmd.ExecuteScalar().ToString();
            }

            // Last SMS Process
            using (var cmd = new SqlCommand(@"
            SELECT TOP 1 SentDate FROM Support_SMSQueue 
            WHERE IsSent = 1 ORDER BY SentDate DESC", conn))
            {
                var lastProcess = cmd.ExecuteScalar();
                lblLastSMSProcess.Text = lastProcess != null ?
                    Convert.ToDateTime(lastProcess).ToString("HH:mm:ss") : "هرگز";
            }

            // Load recent logs
            LoadRecentLogs();
        }
    }

    private void LoadRecentLogs()
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            var query = @"
            SELECT TOP 50 * FROM Support_Logs 
            WHERE CreateDate >= DATEADD(HOUR, -24, GETDATE())
            ORDER BY CreateDate DESC";

            using (var cmd = new SqlCommand(query, conn))
            {
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    gvLogs.DataSource = dt;
                    gvLogs.DataBind();
                }
            }
        }
    }

    protected void btnRefreshLogs_Click(object sender, EventArgs e)
    {
        LoadMonitoringData();
    }

    // Helper methods
    protected string GetLogLevelClass(object logLevel)
    {
        var level = Convert.ToByte(logLevel);
        switch (level)
        {
            case 1: return "info";
            case 2: return "warning";
            case 3: return "danger";
            default: return "secondary";
        }
    }

    protected string GetLogLevelText(object logLevel)
    {
        var level = Convert.ToByte(logLevel);
        switch (level)
        {
            case 1: return "اطلاعات";
            case 2: return "هشدار";
            case 3: return "خطا";
            default: return "نامشخص";
        }
    }

    private void ShowMessage(string message, string type)
    {
        var script = type == "error" ?
            "alert('خطا: " + message + "');" :
            "alert('" + message + "');";

        ScriptManager.RegisterStartupScript(this, GetType(), "message", script, true);
    }
}