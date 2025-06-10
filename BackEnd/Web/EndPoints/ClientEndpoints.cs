using LawyerProject.Application.Clients.Commands;
using LawyerProject.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace LawyerProject.Web.Endpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using LawyerProject.Domain.Entities;
using LawyerProject.Application.Clients;
using LawyerProject.Application.Clients.Queries.GetAllClients;
using LawyerProject.Application.BasicInfo.JudicialDeadline.Queries.GetAllJudges;
using LawyerProject.Application.BasicInfo.JudicialDeadline;
using LawyerProject.Application.Clients.Queries;

public class ClientEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this).RequireAuthorization();
        group.MapGet("Clients", GetAllClients);
        group.MapGet("GetClientCasesByClientId", GetClientCasesByClientId);

        group.MapGet("Clients/{id}", GetClientById)
            .WithName("GetClientById")
            .WithOpenApi();

        group.MapPut("Clients/{id}", UpdateClient)
            .WithName("UpdateClient")
            .WithOpenApi()
            .RequireAuthorization();
    }

    private static async Task<IResult> GetClientById(
        Guid id,
        ISender mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetClientByIdQuery(id);
            var result = await mediator.Send(query, cancellationToken);
            return Results.Ok(result);
        }
        catch (NotFoundException ex)
        {
            return Results.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    private static async Task<IResult> UpdateClient(
        string id,
        [FromBody] UpdateClientCommand command,
        ISender mediator,
        CancellationToken cancellationToken)
    {
        try
        {
            if (id != command.Id)
            {
                return Results.BadRequest("The ID in the URL must match the ID in the request body.");
            }

            await mediator.Send(command, cancellationToken);
            return Results.NoContent();
        }
        catch (NotFoundException ex)
        {
            return Results.NotFound(ex.Message);
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(ex.Errors);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    public async Task<IResult> GetAllClients(ISender sender, [AsParameters] GetAllClientsQuery command)
    {
        var clients = await sender.Send(command);
        return TypedResults.Ok(clients);
    }

    public async Task<Ok<ClientCaseVm>> GetClientCasesByClientId(ISender sender, [AsParameters] GetAllClientCasesQuery command)
    {
        var vm = await sender.Send(command);
        return TypedResults.Ok(vm);
    }

}
