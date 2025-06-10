using System;
using System.IO;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using LawyerProject.Application.Common.Interfaces;
using LawyerProject.Application.Common.Results;
using LawyerProject.Application.Files.Commands;
using LawyerProject.Application.Files.Queries;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LawyerProject.Web.Endpoints;

public class FileEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this).RequireAuthorization();

        // مسیرهای آپلود و دانلود فایل (نیاز به احراز هویت)
        group.MapPost("/upload", UploadFile).DisableAntiforgery();
        group.MapGet("/download/{fileId}", DownloadFile);
        group.MapGet("/image/{fileId}", GetImage);
        group.MapGet("/secure-access/{fileId}", GenerateSecureAccess);
        group.MapGet("/case-files/{caseId}", GetCaseFiles);
        group.MapDelete("/{fileId}", DeleteFile);

        // مسیر دسترسی با توکن (بدون نیاز به احراز هویت)
        app.MapGet("/api/files/access/{token}", AccessSecureFile).AllowAnonymous();
    }

    [IgnoreAntiforgeryToken]
    public async Task<Results<Ok<FileDto>, BadRequest<string>>> UploadFile(
        ISender sender,
        IFormFile file,
        [FromForm] Guid caseId,
        [FromForm] string area,
        [FromForm] string customName,
        HttpContext httpContext,
        IUser user)
    {
        if (file == null || file.Length == 0)
            return TypedResults.BadRequest("No file was uploaded");

        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return TypedResults.BadRequest("User not authenticated");

        using var stream = file.OpenReadStream();
        var command = new UploadFileCommand
        {
            FileStream = stream,
            FileName = file.FileName,
            CustomName = customName,
            CaseId = caseId,
            Area = user.RegionId.ToString(),
            UserId = userId
        };

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return TypedResults.BadRequest(result.Error ?? "Upload failed");

        return TypedResults.Ok(result.Data!);
    }

    public async Task<IResult> DownloadFile(
        ISender sender,
        Guid fileId)
    {
        var query = new DownloadFileQuery { FileId = fileId };
        var result = await sender.Send(query);

        if (!result.Succeeded)
            return Results.BadRequest(result.Error);

        // تبدیل Stream به byte[] برای سازگاری با Results.File
        var memoryStream = new MemoryStream();
        if (result.Stream != null)
        {
            result.Stream.Position = 0;
            await result.Stream.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();

            return Results.File(
                fileBytes,
                result.ContentType,
                fileDownloadName: result.FileName.ToString());
        }

        return Results.BadRequest("File stream is empty");
    }

    public async Task<IResult> GetImage(
        ISender sender,
        Guid fileId)
    {
        var query = new GetImageQuery { FileId = fileId };
        var result = await sender.Send(query);

        if (!result.Succeeded)
            return Results.BadRequest(result.Error);

        // تبدیل Stream به byte[] برای سازگاری با Results.File
        var memoryStream = new MemoryStream();
        if (result.Stream != null)
        {
            result.Stream.Position = 0;
            await result.Stream.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();

            return Results.File(
                fileBytes,
                result.ContentType,
                fileDownloadName: result.FileName.ToString());
        }

        return Results.BadRequest("Image stream is empty");
    }

    public async Task<Results<Ok<FileAccessTokenResult>, BadRequest<string>>> GenerateSecureAccess(
        ISender sender,
        Guid fileId,
        [FromQuery] int expirationMinutes = 30)
    {
        var command = new GenerateSecureAccessTokenCommand
        {
            FileId = fileId,
            ExpirationMinutes = expirationMinutes
        };

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return TypedResults.BadRequest(result.Error ?? "خطا در ساخت لینک دسترسی");

        return TypedResults.Ok(result);
    }

    // در FileEndpoints.cs
    // در FileEndpoints.cs، متد AccessSecureFile را به صورت زیر تغییر دهید:
    public async Task<IResult> AccessSecureFile(
        ISender sender,
        string token,
        HttpContext httpContext)
    {
        if (string.IsNullOrEmpty(token) || !Guid.TryParse(token, out var tokenId))
            return Results.BadRequest("Invalid token");

        var query = new ValidateAccessTokenQuery { TokenId = tokenId };
        var result = await sender.Send(query);

        if (!result.Succeeded)
            return Results.BadRequest(result.Error);

        var memoryStream = new MemoryStream();
        if (result.Stream != null)
        {
            result.Stream.Position = 0;
            await result.Stream.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();

            // ایجاد هدرهای اضافی و تعیین صریح نوع محتوا
            var contentType = result.ContentType ?? "application/pdf";

            httpContext.Response.Headers["Content-Length"] = fileBytes.Length.ToString();
            //httpContext.Response.Headers["Content-Disposition"] = $"inline; filename=\"{result.FileName}\"";
            httpContext.Response.Headers["X-Content-Type-Options"] = "nosniff";

            return Results.File(
                fileBytes,
                contentType,
                result.FileName.ToString(),
                enableRangeProcessing: true,
                lastModified: DateTime.UtcNow,
                entityTag: new Microsoft.Net.Http.Headers.EntityTagHeaderValue($"\"{Guid.NewGuid()}\"")
            );
        }

        return Results.BadRequest("File stream is empty");
    }

    public async Task<Results<Ok<List<FileDto>>, BadRequest<string>>> GetCaseFiles(
        ISender sender,
        Guid caseId,
        [FromQuery] string area,
        IUser user)
    {
        var query = new GetCaseFilesQuery { CaseId = caseId, Area = user.RegionId.ToString() };
        var result = await sender.Send(query);

        if (!result.Succeeded)
            return TypedResults.BadRequest(result.Error ?? "Failed to retrieve files");

        return TypedResults.Ok(result.Data!);
    }

    public async Task<Results<Ok<bool>, BadRequest<string>>> DeleteFile(
        ISender sender,
        Guid fileId)
    {
        var command = new DeleteFileCommand { FileId = fileId };
        var result = await sender.Send(command);

        if (!result.Succeeded)
            return TypedResults.BadRequest(result.Error ?? "Failed to delete file");

        return TypedResults.Ok(true);
    }

}
