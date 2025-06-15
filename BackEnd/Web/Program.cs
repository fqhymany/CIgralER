using LawyerProject.Application;
using LawyerProject.Application.Common.Exceptions;
using LawyerProject.Application.Common.Utils;
using LawyerProject.Infrastructure;
using LawyerProject.Infrastructure.Data;
using LawyerProject.Infrastructure.Extensions;
using LawyerProject.Infrastructure.Hubs;
using LawyerProject.ServiceDefaults;
using LawyerProject.Web;
using LawyerProject.Web.EndPoints;
using LawyerProject.Web.Middleware;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http.Connections; // For HttpTransportType

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("LawyerProjectDb") ?? throw new InvalidOperationException("Connection string 'LawyerProjectDb' not found."); ;
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

builder.AddServiceDefaults();
builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();
builder.Services.AddNotificationServices(builder.Configuration);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.WebRootPath, "uploads")),
    RequestPath = "/uploads"
});
app.UseRouting();
app.UseCors("ReactApp");


app.UseSwaggerUi(settings =>
{
    settings.Path = "/api";
    settings.DocumentPath = "/api/specification.json";
});


app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseMiddleware<AuthorizationMiddleware>();
app.MapRazorPages();
app.MapHub<ChatHub>("/chathub");
app.MapHub<GuestChatHub>("/guestchathub").RequireCors("ReactApp");
app.MapDefaultEndpoints();
app.MapEndpoints();

//app.MapFallbackToFile("index.html");
//app.MapFallbackToFile("/app", "app.html");

// ========== ثبت مسیرها ==========
//۱. صفحه اصلی(لندینگ پیج)
app.MapGet("/", async (HttpContext context) =>
{
    await context.Response.SendFileAsync(
        Path.Combine(app.Environment.WebRootPath, "landing.html")
    );
});

// ۲. تمام مسیرهای /app/* به React واگذار شود
app.MapFallbackToFile("/Home/{*path}", "index.html");

app.UseExceptionHandler(c => c.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var response = new { error = exception?.Message };
    context.Response.StatusCode = exception switch
    {
        UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
        ForbiddenAccessException => StatusCodes.Status403Forbidden,
        NotFoundException => StatusCodes.Status404NotFound,
        ValidationException => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status500InternalServerError
    };
    await context.Response.WriteAsJsonAsync(response);
}));

app.Use(async (context, next) =>
{
    var host = context.Request.Host.Host;
    var subdomain = DomainUtils.GetSubdomain(host);

    if (!string.IsNullOrEmpty(subdomain))
    {
        context.Items["Subdomain"] = subdomain;


        if (context.Request.Headers.TryGetValue("X-Subdomain", out var headerSubdomain))
        {
            context.Items["Subdomain"] = headerSubdomain.ToString();
        }
    }

    await next();
});

//app.Use(async (context, next) =>
//{
//    if (context.Request.Method == "OPTIONS")
//    {
//        context.Response.Headers.Append("Access-Control-Allow-Origin", context.Request.Headers["Origin"].ToString());
//        context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With, X-Subdomain");
//        context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
//        context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
//        context.Response.StatusCode = 200;
//        await context.Response.CompleteAsync();
//        return;
//    }

//    await next.Invoke();
//});

app.Run();

namespace LawyerProject.Web
{
    public partial class Program { }
}
