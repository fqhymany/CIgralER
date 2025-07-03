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
        <li class="nav-item">
            <a class="nav-link" id="agents-tab" data-toggle="tab"
                href="#agents" role="tab">مدیریت پشتیبان‌ها</a>
        </li>
        <li class="nav-item">
            <a class="nav-link" id="monitoring-tab" data-toggle="tab"
                href="#monitoring" role="tab">مانیتورینگ</a>
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

                    <h5 class="card-title">تنظیمات پیامک</h5>

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

                    <h5 class="card-title">تنظیمات پیشرفته</h5>

                    <div class="form-group row">
                        <label class="col-sm-3 col-form-label">تاخیر ارسال پیامک (دقیقه)</label>
                        <div class="col-sm-9">
                            <asp:TextBox ID="txtNotificationDelay" runat="server"
                                CssClass="form-control" TextMode="Number" Text="5" />
                            <small class="form-text text-muted">پیامک‌ها بعد از این مدت عدم فعالیت ارسال می‌شوند
                            </small>
                        </div>
                    </div>

                    <div class="form-group row">
                        <label class="col-sm-3 col-form-label">حداکثر تیکت در ساعت</label>
                        <div class="col-sm-9">
                            <asp:TextBox ID="txtMaxTicketsPerHour" runat="server"
                                CssClass="form-control" TextMode="Number" Text="5" />
                            <small class="form-text text-muted">برای جلوگیری از spam
                            </small>
                        </div>
                    </div>

                    <div class="form-group row">
                        <label class="col-sm-3 col-form-label">تلفن جایگزین</label>
                        <div class="col-sm-9">
                            <asp:TextBox ID="txtAlternativePhone" runat="server"
                                CssClass="form-control" placeholder="02112345678" />
                        </div>
                    </div>

                    <div class="form-group row">
                        <label class="col-sm-3 col-form-label">ایمیل جایگزین</label>
                        <div class="col-sm-9">
                            <asp:TextBox ID="txtAlternativeEmail" runat="server"
                                CssClass="form-control" placeholder="support@company.com" />
                        </div>
                    </div>

                    <hr />

                    <h5 class="card-title">تنظیمات اختصاص خودکار</h5>

                    <div class="form-group row">
                        <label class="col-sm-3 col-form-label">اختصاص خودکار</label>
                        <div class="col-sm-9">
                            <div class="custom-control custom-switch">
                                <asp:CheckBox ID="chkAutoAssignment" runat="server"
                                    CssClass="custom-control-input" Checked="true" />
                                <label class="custom-control-label" for="chkAutoAssignment">
                                    فعال / غیرفعال
                                </label>
                            </div>
                        </div>
                    </div>

                    <div class="form-group row">
                        <label class="col-sm-3 col-form-label">زمان انتظار پذیرش (ثانیه)</label>
                        <div class="col-sm-9">
                            <asp:TextBox ID="txtAgentResponseTimeout" runat="server"
                                CssClass="form-control" TextMode="Number" Text="60" />
                        </div>
                    </div>

                    <div class="form-group row">
                        <label class="col-sm-3 col-form-label">حداکثر تلاش برای اختصاص</label>
                        <div class="col-sm-9">
                            <asp:TextBox ID="txtMaxAssignmentAttempts" runat="server"
                                CssClass="form-control" TextMode="Number" Text="3" />
                        </div>
                    </div>

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

        <!-- پشتیبان‌ها -->
        <div class="tab-pane fade" id="agents" role="tabpanel">
            <div class="card">
                <div class="card-body">
                    <h5 class="card-title">مدیریت پشتیبان‌ها</h5>

                    <asp:GridView ID="gvAgents" runat="server"
                        CssClass="table table-striped"
                        AutoGenerateColumns="false"
                        OnRowCommand="gvAgents_RowCommand"
                        DataKeyNames="Id">
                        <columns>
                            <asp:BoundField DataField="UserFullName" HeaderText="نام" />
                            <asp:BoundField DataField="UserMobile" HeaderText="موبایل" />
                            <asp:TemplateField HeaderText="وضعیت">
                                <itemtemplate>
                                    <span class="badge <%# (bool)Eval("IsActive") ? "badge-success" : "badge-secondary" %>">
                                        <%# (bool)Eval("IsActive") ? "فعال" : "غیرفعال" %>
                                    </span>
                                    <span class="badge <%# (bool)Eval("IsOnline") ? "badge-primary" : "badge-light" %>">
                                        <%# (bool)Eval("IsOnline") ? "آنلاین" : "آفلاین" %>
                                    </span>
                                </itemtemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="CurrentActiveTickets" HeaderText="تیکت‌های فعال" />
                            <asp:BoundField DataField="MaxConcurrentTickets" HeaderText="حداکثر ظرفیت" />
                            <asp:BoundField DataField="TotalHandledTickets" HeaderText="کل تیکت‌ها" />
                            <asp:TemplateField HeaderText="عملیات">
                                <itemtemplate>
                                    <asp:LinkButton ID="btnEditAgent" runat="server"
                                        CommandName="EditAgent"
                                        CommandArgument='<%# Eval("Id") %>'
                                        CssClass="btn btn-sm btn-info">
                                        <i class="fa fa-edit"></i>ویرایش
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="btnToggleAgent" runat="server"
                                        CommandName="ToggleAgent"
                                        CommandArgument='<%# Eval("Id") %>'
                                        CssClass="btn btn-sm btn-warning">
                                        <i class="fa fa-power-off"></i>
                                        <%# (bool)Eval("IsActive") ? "غیرفعال" : "فعال" %>
                                    </asp:LinkButton>
                                </itemtemplate>
                            </asp:TemplateField>
                        </columns>
                    </asp:GridView>

                    <hr />

                    <h6>افزودن پشتیبان جدید</h6>
                    <div class="form-group">
                        <label>انتخاب کاربر</label>
                        <asp:DropDownList ID="ddlNewAgent" runat="server"
                            CssClass="form-control"
                            DataTextField="FullName"
                            DataValueField="Id" />
                    </div>

                    <div class="form-group">
                        <label>حداکثر تیکت همزمان</label>
                        <asp:TextBox ID="txtNewAgentMaxTickets" runat="server"
                            CssClass="form-control" TextMode="Number" Text="5" />
                    </div>

                    <div class="form-group">
                        <label>تخصص‌ها (با کاما جدا کنید)</label>
                        <asp:TextBox ID="txtNewAgentSpecialties" runat="server"
                            CssClass="form-control"
                            placeholder="فنی، مالی، عمومی" />
                    </div>

                    <asp:Button ID="btnAddAgent" runat="server"
                        Text="افزودن پشتیبان"
                        CssClass="btn btn-primary"
                        OnClick="btnAddAgent_Click" />
                </div>
            </div>
        </div>

        <!-- محتوای تب مانیتورینگ -->
        <div class="tab-pane fade" id="monitoring" role="tabpanel">
            <div class="card">
                <div class="card-body">
                    <h5 class="card-title">مانیتورینگ سیستم</h5>

                    <!-- وضعیت پردازش‌های پس‌زمینه -->
                    <div class="row">
                        <div class="col-md-6">
                            <h6>وضعیت پردازش‌ها</h6>
                            <table class="table table-sm">
                                <tr>
                                    <td>آخرین پردازش SMS:</td>
                                    <td>
                                        <asp:Label ID="lblLastSMSProcess" runat="server" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>SMS در صف:</td>
                                    <td>
                                        <asp:Label ID="lblPendingSMS" runat="server" />
                                    </td>
                                </tr>
                                <tr>
                                    <td>درخواست‌های منقضی شده:</td>
                                    <td>
                                        <asp:Label ID="lblExpiredRequests" runat="server" />
                                    </td>
                                </tr>
                            </table>
                        </div>

                        <div class="col-md-6">
                            <h6>آمار زمان پاسخ</h6>
                            <canvas id="responseTimeChart"></canvas>
                        </div>
                    </div>

                    <hr />

                    <!-- لاگ‌های سیستم -->
                    <h6>آخرین لاگ‌ها</h6>
                    <asp:GridView ID="gvLogs" runat="server"
                        CssClass="table table-sm table-striped"
                        AutoGenerateColumns="false">
                        <columns>
                            <asp:BoundField DataField="CreateDate" HeaderText="زمان"
                                DataFormatString="{0:HH:mm:ss}" />
                            <asp:TemplateField HeaderText="سطح">
                                <itemtemplate>
                                    <span class="badge badge-<%# GetLogLevelClass(Eval("LogLevel")) %>">
                                        <%# GetLogLevelText(Eval("LogLevel")) %>
                                    </span>
                                </itemtemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Message" HeaderText="پیام" />
                            <asp:BoundField DataField="UserId" HeaderText="کاربر" />
                        </columns>
                    </asp:GridView>

                    <asp:Button ID="btnRefreshLogs" runat="server"
                        Text="بازخوانی"
                        CssClass="btn btn-sm btn-secondary"
                        OnClick="btnRefreshLogs_Click" />
                </div>
            </div>
        </div>

    </div>
</div>
<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
<script>
    // Response Time Chart
    var ctx = document.getElementById('responseTimeChart').getContext('2d');
    var responseTimeChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: ['00:00', '04:00', '08:00', '12:00', '16:00', '20:00'],
            datasets: [{
                label: 'میانگین زمان پاسخ (دقیقه)',
                data: [2, 3, 5, 8, 6, 4],
                borderColor: 'rgb(75, 192, 192)',
                tension: 0.1
            }]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });

    // Auto refresh monitoring data
    setInterval(function () {
        $.get('/Support/GetMonitoringData.ashx', function (data) {
            $('#lblPendingSMS').text(data.pendingSMS);
            $('#lblExpiredRequests').text(data.expiredRequests);
            // Update chart if needed
        });
    }, 30000); // Every 30 seconds
</script>
