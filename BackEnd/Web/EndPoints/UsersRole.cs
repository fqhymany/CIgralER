using LawyerProject.Application.UserRoles.Queries.GetUserNamesByRoleName;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LawyerProject.Web.Endpoints;
public class UsersRole : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        //app.MapGroup(this)
        //    .RequireAuthorization()
        //    .MapGet("initialdata", GetInitialCaseRegistrationData)
        //    .MapGet("all", GetCases)
        //    .MapPost(CreateCase)
        //    .MapPut(UpdateCase, "{id}")
        //    .MapDelete(DeleteCase, "{id}");
        var group = app.MapGroup(this).RequireAuthorization();
        group.MapGet("GetUserNamesByRoleName", GetUserNamesByRoleName);
    }
    public async Task<Ok<UserRoleInfoVm>> GetUserNamesByRoleName(ISender sender, [AsParameters] GetUserNamesByRoleNameCommand command)
    {
        var vm = await sender.Send(command);
        return TypedResults.Ok(vm);
    }
}
