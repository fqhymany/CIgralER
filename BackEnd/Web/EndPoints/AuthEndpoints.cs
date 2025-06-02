using System.Security.Claims;
using System.Text.RegularExpressions;
using LawyerProject.Application.Auth.Commands.ChangePassword.cs;
using LawyerProject.Application.Auth.Commands.DeleteUser;
using LawyerProject.Application.Auth.Commands.ForgotPassword;
using LawyerProject.Application.Auth.Commands.Login;
using LawyerProject.Application.Auth.Commands.Logout;
using LawyerProject.Application.Auth.Commands.RefreshToken;
using LawyerProject.Application.Auth.Commands.Register;
using LawyerProject.Application.Auth.Commands.ResetPassword;
using LawyerProject.Application.Auth.Commands.Select_Region;
using LawyerProject.Application.Auth.Commands.UpdateProfile;
using LawyerProject.Application.Auth.Queries.GetRole;
using LawyerProject.Application.Auth.Queries.GetUser;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace LawyerProject.Web.Endpoints;

public class AuthEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this);

        // Public endpoints
        group.MapPost("/login", Login).AllowAnonymous();
        group.MapPost("/forgot-password", ForgotPassword).AllowAnonymous();
        group.MapPost("/register", Register).AllowAnonymous();

        // Protected endpoints
        var protectedGroup = group.RequireAuthorization();

        protectedGroup.MapGet("/profile", GetUserProfile);
        protectedGroup.MapPut("/updateProfile/{id}", UpdateProfile);
        protectedGroup.MapPut("/change-password", ChangePassword);
        protectedGroup.MapPost("/refresh-token", RefreshToken);
        protectedGroup.MapPost("/logout", Logout);
        protectedGroup.MapPost("/select-region", SelectRegion);
        protectedGroup.MapPost("/deleteUsers/{id}", DeleteUser);
        protectedGroup.MapGet("/roles", GetAllRoles);
        protectedGroup.MapPost("/reset-password", ResetPassword);
    }

    public async Task<Results<Ok<AuthResponseDto>, BadRequest<string>>> Login(
        ISender sender,
        LoginCommand command)
    {
        var result = await sender.Send(command);
        if (result.Succeeded)
            return TypedResults.Ok(result.Data);

        return TypedResults.BadRequest("نام کاربری یا رمزعبور اشتباه است");
    }

    public async Task<Results<Ok<AuthResponseDto>, BadRequest<string>>> SelectRegion(
        ISender sender,
        SelectRegionCommand command,
        HttpContext httpContext)
    {
        // استخراج userId از توکن
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return TypedResults.BadRequest("کاربر احراز هویت نشده است");

        // استفاده از دستور with برای ایجاد نمونه جدید با userId
        var newCommand = command with { UserId = userId };

        var result = await sender.Send(newCommand);
        if (result.Succeeded)
            return TypedResults.Ok(result.Data);

        return TypedResults.BadRequest(result.Error);
    }

    public async Task<Results<Ok<AuthResponseDto>, BadRequest<string>>> Register(
        ISender sender,
        RegisterCommand command)
    {
        var result = await sender.Send(command);
        if (result.Succeeded)
            return TypedResults.Ok(result.Data);

        return TypedResults.BadRequest(result.Error);
    }

    public async Task<Results<Ok<UserProfileDto>, NotFound>> GetUserProfile(
        ISender sender,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return TypedResults.NotFound();

        var query = new GetUserProfileQuery(userId);
        var result = await sender.Send(query);

        if (result is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(result);
    }

    public async Task<Results<NoContent, BadRequest<string>>> UpdateProfile(
        string id,
        ISender sender,
        UpdateProfileCommand command,
        [FromHeader(Name = "Authorization")] string authorization)
    {

        if (id != command.UserId)
        {
            return (Results<NoContent, BadRequest<string>>)Results.BadRequest("کاربری با این شناسه پیدا نشد");
        }
        var userId = command.UserId;
        if (string.IsNullOrEmpty(userId))
            return TypedResults.BadRequest("کاربری با این شناسه پیدا نشد");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return TypedResults.BadRequest(result.Error);

        return TypedResults.NoContent();
    }

    public async Task<Results<Ok<AuthResponseDto>, BadRequest<string>>> RefreshToken(
        ISender sender,
        RefreshTokenCommand command)
    {
        var result = await sender.Send(command);
        if (result.Succeeded)
            return TypedResults.Ok(result.Data);

        return TypedResults.BadRequest(result.Error);
    }

    public async Task<Results<Ok<string>, BadRequest<string>>> ForgotPassword(
        ISender sender,
        ForgotPasswordCommand command)
    {
        var result = await sender.Send(command);
        if (result.Succeeded)
            return TypedResults.Ok("Password reset email sent");

        return TypedResults.BadRequest(result.Errors[0]);
    }

    public async Task<Results<NoContent, BadRequest<string>>> ResetPassword(
        ISender sender,
        ResetPasswordCommand command)
    {
        var result = await sender.Send(command);
        if (!result.Succeeded)
            return TypedResults.BadRequest(result.Errors[0]);

        return TypedResults.NoContent();
    }

    public async Task<Results<Ok<IList<RoleDto>>, BadRequest<string>>> GetAllRoles(
        ISender sender)
    {
        try
        {
            var roles = await sender.Send(new GetAllRolesQuery());
            return TypedResults.Ok(roles);
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }

    public async Task<Results<NoContent, BadRequest<string>>> ChangePassword(
        ISender sender,
        ChangePasswordCommand command,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId != command.UserId)
            return TypedResults.BadRequest("Invalid user");

        var result = await sender.Send(command);
        if (!result.Succeeded)
            return TypedResults.BadRequest(result.Errors[0]);

        return TypedResults.NoContent();
    }

    public async Task<NoContent> Logout(
        ISender sender,
        LogoutCommand command)
    {
        await sender.Send(command);
        return TypedResults.NoContent();
    }

    public async Task<Results<NoContent, BadRequest<string>>> DeleteUser(
        ISender sender,
        string id,
        HttpContext httpContext)
    {
        try
        {
            var command = new DeleteUserCommand { Id = id };
            var result = await sender.Send(command);

            if (!result)
                return TypedResults.BadRequest("Failed to delete user");

            return TypedResults.NoContent();
        }
        catch (NotFoundException)
        {
            return TypedResults.BadRequest("User not found");
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(ex.Message);
        }
    }
}
