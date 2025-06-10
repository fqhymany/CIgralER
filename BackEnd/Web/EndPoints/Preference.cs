
using Microsoft.AspNetCore.Http.HttpResults;
using LawyerProject.Application.CaseFinancials.CaseServices.Queries.GetCaseServicesRegData;
using LawyerProject.Application.CaseFinancials.CaseServices.Commands.DeleteCaseService;
using LawyerProject.Application.CaseFinancials.CaseServices.Commands.UpdateCaseService;
using LawyerProject.Application.CaseFinancials.CaseServices.Commands.CreateCaseService;
using LawyerProject.Application.CaseFinancials.CaseServices.Queries.GetCaseServices;
using LawyerProject.Application.CaseFinancials.CaseServices;
using LawyerProject.Application.CaseFinancials.CaseServices.Queries.GetCaseService;
using LawyerProject.Application.Cases.Queries.GetCaseById;
using LawyerProject.Application.Cases.Queries.GetCases;
using LawyerProject.Application.CaseFinancials.CasePaymentAgreements.Queries.GetCasePayAgreesRegData;
using LawyerProject.Application.CaseFinancials.CasePaymentAgreements.Commands.CreateCasePaymentAgreement;
using LawyerProject.Application.CaseFinancials.CasePaymentAgreements.Commands.UpdateCasePaymentAgreement;
using LawyerProject.Application.CaseFinancials.CasePaymentAgreements.Commands.DeleteCasePaymentAgreement;
using LawyerProject.Domain.Entities.CaseFinancials;
using LawyerProject.Application.CaseFinancials.CasePaymentAgreements.Queries.GetCasePaymentAgreement;
using LawyerProject.Application.CaseFinancials.CasePaymentAgreements;
using LawyerProject.Application.CaseFinancials.CasePaymentTransactions.Queries.GetCasePayTnxsRegData;
using LawyerProject.Application.CaseFinancials.CasePaymentTransactions.Queries;
using LawyerProject.Application.CaseFinancials.CasePaymentTransactions;
using LawyerProject.Application.CaseFinancials.CasePaymentTransactions.Commands.CreateCasePaymentTransaction;
using LawyerProject.Application.CaseFinancials.CasePaymentTransactions.Commands.UpdateCasePaymentTransaction;
using LawyerProject.Application.CaseFinancials.CasePaymentTransactions.Commands.DeleteCasePaymentTransaction;
using LawyerProject.Application.Preferences.Queries;
using LawyerProject.Application.Preferences.Commands;
namespace LawyerProject.Web.Endpoints;
public class Preference : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this).RequireAuthorization();
        group.MapGet("GetUserPreferencesByKey", GetUserPreferencesByKey);
        group.MapPost("UpdateUserPreference", UpdateUserPreference);
        
    }
    public async Task<Ok<string>> GetUserPreferencesByKey(ISender sender, [AsParameters] GetUserPreferencesByKeyQuery command)
    {
        var vm = await sender.Send(command);
        return TypedResults.Ok(vm);
    }
    public async Task<IResult> UpdateUserPreference(UpdateUserPreferenceCommand command, ISender sender)
    {
        var updatedJudgeId = await sender.Send(command);

        if (updatedJudgeId == 0)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(updatedJudgeId);
    }

}
