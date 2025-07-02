using System;
using Rubik_Support.BLL;

public partial class Modules_Support_Default : System.Web.UI.Page
{
    private SupportBLL _bll = new SupportBLL();

    public int CurrentUserId
    {
        get
        {
            return Convert.ToInt32(Session["UserId"] ?? 0);
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // Check access
            if (!_bll.HasSupportAccess(CurrentUserId))
            {
                Response.Redirect("~/AccessDenied.aspx");
                return;
            }

            // Check if module is enabled
            if (!_bll.IsModuleEnabled())
            {
                Response.Redirect("~/ModuleDisabled.aspx");
                return;
            }
        }
    }
}