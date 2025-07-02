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

            // Show success message
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

}