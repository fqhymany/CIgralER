<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Admin.aspx.cs" Inherits="Modules_Support_Admin" %>

<style>
    /* Admin Panel Styles */
.stat-box {
    padding: 20px;
    border-radius: 10px;
    text-align: center;
    margin-bottom: 20px;
}

    .stat-box h3 {
        font-size: 2.5rem;
        margin-bottom: 10px;
    }

    .stat-box p {
        margin: 0;
        font-size: 1.1rem;
    }

.nav-tabs {
    border-bottom: 2px solid #dee2e6;
}

    .nav-tabs .nav-link {
        border: none;
        color: #495057;
        padding: 10px 20px;
    }

        .nav-tabs .nav-link.active {
            color: #007bff;
            border-bottom: 2px solid #007bff;
            background-color: transparent;
        }

.card {
    border: none;
    box-shadow: 0 0 20px rgba(0,0,0,0.08);
}

.card-title {
    color: #333;
    font-weight: 600;
}

.table th {
    background-color: #f8f9fa;
    border-bottom: 2px solid #dee2e6;
    color: #495057;
    font-weight: 600;
}

.custom-control-label {
    padding-right: 1.5rem;
}

/* RTL adjustments */
body[dir="rtl"] .custom-control-label {
    padding-right: 0;
    padding-left: 1.5rem;
}
</style>
    <div class="container-fluid">
        <h2 class="mb-4">مدیریت سیستم پشتیبانی</h2>

        <ul class="nav nav-tabs" id="adminTabs" role="tablist">
            <li class="nav-item">
                <a class="nav-link active" id="settings-tab" data-toggle="tab"
                    href="#settings" role="tab">تنظیمات</a>
            </li>
            <li class="nav-item">
                <a class="nav-link" id="users-tab" data-toggle="tab"
                    href="#users" role="tab">کاربران پشتیبان</a>
            </li>
            <li class="nav-item">
                <a class="nav-link" id="tickets-tab" data-toggle="tab"
                    href="#tickets" role="tab">مدیریت تیکت‌ها</a>
            </li>
            <li class="nav-item">
                <a class="nav-link" id="reports-tab" data-toggle="tab"
                    href="#reports" role="tab">گزارشات</a>
            </li>
        </ul>

        <div class="tab-content mt-3" id="adminTabContent">
            <!-- تنظیمات -->
            <div class="tab-pane fade show active" id="settings" role="tabpanel">
                <div class="card">
                    <div class="card-body">
                        <h5 class="card-title">تنظیمات عمومی</h5>

                        <div class="form-group row">
                            <label class="col-sm-3 col-form-label">وضعیت ماژول</label>
                            <div class="col-sm-9">
                                <div class="custom-control custom-switch">
                                    <asp:CheckBox ID="chkModuleEnabled" runat="server"
                                        CssClass="custom-control-input" />
                                    <label class="custom-control-label" for="chkModuleEnabled">
                                        فعال / غیرفعال
                                   
                                    </label>
                                </div>
                            </div>
                        </div>

                        <hr />

                        <h5 class="card-title">تنظیمات پیامک (کاونگار)</h5>

                        <div class="form-group row">
                            <label class="col-sm-3 col-form-label">API Key</label>
                            <div class="col-sm-9">
                                <asp:TextBox ID="txtKavenegarApiKey" runat="server"
                                    CssClass="form-control" />
                            </div>
                        </div>

                        <div class="form-group row">
                            <label class="col-sm-3 col-form-label">شماره فرستنده</label>
                            <div class="col-sm-9">
                                <asp:TextBox ID="txtKavenegarSender" runat="server"
                                    CssClass="form-control" />
                            </div>
                        </div>

                        <hr />

                        <h5 class="card-title">تنظیمات فایل</h5>

                        <div class="form-group row">
                            <label class="col-sm-3 col-form-label">حداکثر حجم فایل (MB)</label>
                            <div class="col-sm-9">
                                <asp:TextBox ID="txtMaxFileSize" runat="server"
                                    CssClass="form-control" TextMode="Number" />
                            </div>
                        </div>

                        <div class="form-group row">
                            <label class="col-sm-3 col-form-label">پسوندهای مجاز</label>
                            <div class="col-sm-9">
                                <asp:TextBox ID="txtAllowedFileTypes" runat="server"
                                    CssClass="form-control" />
                                <small class="form-text text-muted">پسوندها را با کاما جدا کنید. مثال: .jpg,.pdf,.doc
                                </small>
                            </div>
                        </div>

                        <div class="form-group row">
                            <div class="col-sm-9 offset-sm-3">
                                <asp:Button ID="btnSaveSettings" runat="server"
                                    Text="ذخیره تنظیمات"
                                    CssClass="btn btn-primary"
                                    OnClick="btnSaveSettings_Click" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- کاربران پشتیبان -->
            <div class="tab-pane fade" id="users" role="tabpanel">
                <div class="card">
                    <div class="card-body">
                        <h5 class="card-title">مدیریت دسترسی پشتیبان‌ها</h5>

                        <div class="form-group">
                            <label>گروه پشتیبان‌ها</label>
                            <asp:DropDownList ID="ddlSupportGroup" runat="server"
                                CssClass="form-control"
                                DataTextField="Name"
                                DataValueField="id" />
                            <small class="form-text text-muted">کاربران عضو این گروه به عنوان پشتیبان شناخته می‌شوند
                            </small>
                        </div>

                        <asp:Button ID="btnSaveSupportGroup" runat="server"
                            Text="ذخیره"
                            CssClass="btn btn-primary"
                            OnClick="btnSaveSupportGroup_Click" />

                        <hr />

                        <h5 class="card-title mt-4">لیست پشتیبان‌های فعلی</h5>

                        <asp:GridView ID="gvSupportUsers" runat="server"
                            CssClass="table table-striped"
                            AutoGenerateColumns="false">
                            <columns>
                                <asp:BoundField DataField="UserName" HeaderText="نام کاربری" />
                                <asp:BoundField DataField="FullName" HeaderText="نام و نام خانوادگی" />
                                <asp:BoundField DataField="Mobile" HeaderText="موبایل" />
                                <asp:BoundField DataField="Email" HeaderText="ایمیل" />
                            </columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>

            <!-- مدیریت تیکت‌ها -->
            <div class="tab-pane fade" id="tickets" role="tabpanel">
                <div class="card">
                    <div class="card-body">
                        <h5 class="card-title">جستجو و مدیریت تیکت‌ها</h5>

                        <div class="row mb-3">
                            <div class="col-md-3">
                                <label>شماره تیکت</label>
                                <asp:TextBox ID="txtSearchTicketNumber" runat="server"
                                    CssClass="form-control" />
                            </div>
                            <div class="col-md-3">
                                <label>شماره موبایل</label>
                                <asp:TextBox ID="txtSearchMobile" runat="server"
                                    CssClass="form-control" />
                            </div>
                            <div class="col-md-3">
                                <label>وضعیت</label>
                                <asp:DropDownList ID="ddlSearchStatus" runat="server"
                                    CssClass="form-control">
                                    <asp:ListItem Value="">همه</asp:ListItem>
                                    <asp:ListItem Value="1">باز</asp:ListItem>
                                    <asp:ListItem Value="2">در حال بررسی</asp:ListItem>
                                    <asp:ListItem Value="3">بسته شده</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="col-md-3">
                                <label>&nbsp;</label>
                                <asp:Button ID="btnSearchTickets" runat="server"
                                    Text="جستجو"
                                    CssClass="btn btn-primary btn-block"
                                    OnClick="btnSearchTickets_Click" />
                            </div>
                        </div>

                        <asp:GridView ID="gvTickets" runat="server"
                            CssClass="table table-striped"
                            AutoGenerateColumns="false"
                            OnRowCommand="gvTickets_RowCommand"
                            DataKeyNames="Id">
                            <columns>
                                <asp:BoundField DataField="TicketNumber" HeaderText="شماره تیکت" />
                                <asp:BoundField DataField="VisitorMobile" HeaderText="موبایل" />
                                <asp:BoundField DataField="Subject" HeaderText="موضوع" />
                                <asp:BoundField DataField="StatusText" HeaderText="وضعیت" />
                                <asp:BoundField DataField="CreateDate" HeaderText="تاریخ ایجاد"
                                    DataFormatString="{0:yyyy/MM/dd HH:mm}" />
                                <asp:TemplateField HeaderText="عملیات">
                                    <itemtemplate>
                                        <asp:LinkButton ID="btnView" runat="server"
                                            CommandName="ViewTicket"
                                            CommandArgument='<%# Eval("Id") %>'
                                            CssClass="btn btn-sm btn-info">
                                            <i class="fa fa-eye"></i>مشاهده
                                       
                                        </asp:LinkButton>
                                        <asp:LinkButton ID="btnDelete" runat="server"
                                            CommandName="DeleteTicket"
                                            CommandArgument='<%# Eval("Id") %>'
                                            CssClass="btn btn-sm btn-danger"
                                            OnClientClick="return confirm('آیا اطمینان دارید؟');">
                                            <i class="fa fa-trash"></i>حذف
                                       
                                        </asp:LinkButton>
                                    </itemtemplate>
                                </asp:TemplateField>
                            </columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>

            <!-- گزارشات -->
            <div class="tab-pane fade" id="reports" role="tabpanel">
                <div class="card">
                    <div class="card-body">
                        <h5 class="card-title">گزارشات و آمار</h5>

                        <div class="row">
                            <div class="col-md-3">
                                <div class="stat-box bg-info text-white">
                                    <h3>
                                        <asp:Label ID="lblTotalTickets" runat="server" Text="0" />
                                    </h3>
                                    <p>کل تیکت‌ها</p>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="stat-box bg-warning text-white">
                                    <h3>
                                        <asp:Label ID="lblOpenTickets" runat="server" Text="0" />
                                    </h3>
                                    <p>تیکت‌های باز</p>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="stat-box bg-success text-white">
                                    <h3>
                                        <asp:Label ID="lblClosedToday" runat="server" Text="0" />
                                    </h3>
                                    <p>بسته شده امروز</p>
                                </div>
                            </div>
                            <div class="col-md-3">
                                <div class="stat-box bg-primary text-white">
                                    <h3>
                                        <asp:Label ID="lblActiveSupports" runat="server" Text="0" />
                                    </h3>
                                    <p>پشتیبان‌های فعال</p>
                                </div>
                            </div>
                        </div>

                        <hr />

                        <h5 class="card-title mt-4">گزارش عملکرد پشتیبان‌ها</h5>

                        <asp:GridView ID="gvSupportPerformance" runat="server"
                            CssClass="table table-striped"
                            AutoGenerateColumns="false">
                            <columns>
                                <asp:BoundField DataField="SupportName" HeaderText="نام پشتیبان" />
                                <asp:BoundField DataField="TotalTickets" HeaderText="کل تیکت‌ها" />
                                <asp:BoundField DataField="ClosedTickets" HeaderText="بسته شده" />
                                <asp:BoundField DataField="AvgResponseTime" HeaderText="میانگین زمان پاسخ" />
                                <asp:BoundField DataField="Rating" HeaderText="امتیاز" />
                            </columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </div>
    </div>
