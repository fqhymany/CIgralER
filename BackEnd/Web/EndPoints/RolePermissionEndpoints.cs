using LawyerProject.Application.RolePermissions.Commands.SetRolePermissions;
using LawyerProject.Application.RolePermissions.DTOs;
using LawyerProject.Application.RolePermissions.Queries.GetPermissionsByRole;
using LawyerProject.Application.RolePermissions.Queries.GetSections;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LawyerProject.Web.Endpoints;

public class RolePermissionEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/role-permissions").RequireAuthorization();
        group.MapGet("sections", GetSections);
        group.MapGet("{roleId}", GetPermissionsByRole);
        group.MapPost("set", SetRolePermissions);
    }

    // لیست بخش‌های سیستم
    public static async Task<IResult> GetSections([FromServices] IMediator mediator)
    {
        var sections = await mediator.Send(new GetSectionsQuery());
        return Results.Ok(sections);
    }

    // دریافت دسترسی‌های یک نقش
    public static async Task<IResult> GetPermissionsByRole([FromServices] IMediator mediator, string roleId)
    {
        var permissions = await mediator.Send(new GetPermissionsByRoleQuery(roleId));
        return Results.Ok(permissions);
    }

    // ثبت یا ویرایش دسترسی‌های یک نقش
    public static async Task<IResult> SetRolePermissions([FromServices] IMediator mediator, [FromBody] SetRolePermissionsDto dto)
    {
        var command = new SetRolePermissionsCommand
        {
            RoleId = dto.RoleId,
            Permissions = dto.Permissions
        };

        await mediator.Send(command);
        return Results.Ok();
    }
}
