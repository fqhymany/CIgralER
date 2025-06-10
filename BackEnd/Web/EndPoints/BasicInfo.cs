using LawyerProject.Application.BasicInfo.CaseType.Queries.GetAllCaseTypes;
using Microsoft.AspNetCore.Http.HttpResults;
using LawyerProject.Application.BasicInfo.CaseType.Queries.GetAllCaseStatuses;
using LawyerProject.Domain.Entities;
using LawyerProject.Application.BasicInfo.CaseTypes.Commands;
using LawyerProject.Application.BasicInfo.CaseStatuses.Commands;
using LawyerProject.Application.BasicInfo.CasePredefinedSubjects.Queries;
using LawyerProject.Application.BasicInfo.CasePredefinedSubject.Commands;
using LawyerProject.Application.BasicInfo.ClientRolesInCase.Queries;
using LawyerProject.Application.BasicInfo.ClientRolesInCase.Commands;
using LawyerProject.Application.BasicInfo.Judge.Queries.GetAllJudges;
using LawyerProject.Application.BasicInfo.Judges.Commands;
using LawyerProject.Application.BasicInfo.CourtType;
using LawyerProject.Application.BasicInfo.CourtType.Queries.GetAllCourtTypes;
using LawyerProject.Application.BasicInfo.CourtTypes.Commands;
using LawyerProject.Application.BasicInfo.CourtSubTypes.Queries;
using LawyerProject.Application.BasicInfo.CourtSubTypes.Commands;
using LawyerProject.Application.BasicInfo.CasePredefinedSubjects.Commands;
using LawyerProject.Application.BasicInfo.CasePredefinedSubjects;
using LawyerProject.Application.BasicInfo.CaseStatuses;
using LawyerProject.Application.BasicInfo.CaseTypes;
using LawyerProject.Application.BasicInfo.ClientRolesInCase;
using LawyerProject.Application.BasicInfo.Judges;
using LawyerProject.Application.BasicInfo.CourtSubTypes;
using Microsoft.AspNetCore.Mvc;

namespace LawyerProject.Web.Endpoints
{
    public class BasicInfo : EndpointGroupBase
    {
        public override void Map(WebApplication app)
        {
            var group = app.MapGroup(this).RequireAuthorization();

            // Mapping routes for BasicInfo
            group.MapGet("CaseType", GetAllCaseTypes);
            group.MapGet("CaseTypeForManage", GetAllCaseTypesForManage);
            group.MapPost("CaseType", CreateCaseType);
            group.MapPost("CaseType/Update", UpdateCaseType);
            group.MapPost("CaseType/Delete", DeleteCaseType);

            group.MapGet("CaseStatus", GetAllCaseStatuses);
            group.MapPost("CaseStatus", CreateCaseStatus);
            group.MapPost("CaseStatus/Update", UpdateCaseStatus);
            group.MapPost("CaseStatus/Delete", DeleteCaseStatus);

            group.MapGet("CasePredefinedSubject", GetAllCasePredefinedSubjects);
            group.MapPost("CasePredefinedSubject", CreateCasePredefinedSubject);
            group.MapPost("CasePredefinedSubject/Update", UpdateCasePredefinedSubject);
            group.MapPost("CasePredefinedSubject/Delete", DeleteCasePredefinedSubject);

            group.MapGet("ClientRoleInCase", GetAllClientRolesInCase);
            group.MapPost("ClientRoleInCase", CreateClientRoleInCase);
            group.MapPost("ClientRoleInCase/Update", UpdateClientRoleInCase);
            group.MapPost("ClientRoleInCase/Delete", DeleteClientRoleInCase);

            group.MapGet("Judge", GetAllJudges);
            group.MapPost("Judge", CreateJudge);
            group.MapPost("Judge/Update", UpdateJudge);
            group.MapPost("Judge/Delete", DeleteJudge);

            group.MapGet("CourtType", GetAllCourtTypes);
            group.MapPost("CourtType", CreateCourtType);
            group.MapPost("CourtType/Update", UpdateCourtType);
            group.MapPost("CourtType/Delete", DeleteCourtType);

            group.MapGet("CourtSubType", GetAllCourtSubTypes);
            group.MapPost("CourtSubType", CreateCourtSubType);
            group.MapPost("CourtSubType/Update", UpdateCourtSubType);
            group.MapPost("CourtSubType/Delete", DeleteCourtSubType);


        }

        // Handler for fetching all CaseTypes
        public async Task<Results<Ok<List<CaseTypeDto>>, NotFound>> GetAllCaseTypes(ISender sender)
        {
            var caseTypes = await sender.Send(new GetAllCaseTypesQuery());

            return TypedResults.Ok(caseTypes);
        }
        public async Task<Results<Ok<List<CaseTypeDto>>, NotFound>> GetAllCaseTypesForManage(ISender sender)
        {
            var caseTypes = await sender.Send(new GetAllCaseTypesQuery(IncludeSystemRecords:false));

            return TypedResults.Ok(caseTypes);
        }
        // Handler for creating a new CaseType
        public static async Task<IResult> CreateCaseType(CreateCaseTypeCommand command, ISender sender)
        {
            if (string.IsNullOrEmpty(command.Name))
            {
                return TypedResults.BadRequest("Name is required");
            }

            var caseTypeId = await sender.Send(command);

            return TypedResults.Created($"/api/BasicInfo/CaseType/{caseTypeId}");
        }

        // Handler for updating an existing CaseType
        public async Task<IResult> UpdateCaseType([FromBody] UpdateCaseTypeCommand command, ISender sender)
        {
            if (string.IsNullOrEmpty(command.Name))
            {
                return TypedResults.BadRequest("The Name field is required.");
            }

            var updatedCaseTypeId = await sender.Send(command);

            if (updatedCaseTypeId == 0)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new CaseTypeDto { Id = updatedCaseTypeId, Name = command.Name });
        }


        // Handler for deleting a CaseType
        public static async Task<IResult> DeleteCaseType([FromBody] DeleteCaseTypeCommand command, ISender sender)
        {
            var result = await sender.Send(command);

            if (!result)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new { Message = "CaseType deleted successfully" });
        }

        // Handler for fetching all CaseStatuses
        public async Task<Results<Ok<List<CaseStatusDto>>, NotFound>> GetAllCaseStatuses(ISender sender)
        {
            var caseStatuses = await sender.Send(new GetAllCaseStatusesQuery());

            /*if (caseStatuses == null || !caseStatuses.Any())
            {
                return TypedResults.NotFound();
            }
            */
            return TypedResults.Ok(caseStatuses);
        }


        // Handler for creating a new CaseStatus
        public static async Task<IResult> CreateCaseStatus(CreateCaseStatusCommand command, ISender sender)
        {
            if (string.IsNullOrEmpty(command.Name))
            {
                return TypedResults.BadRequest("Name is required");
            }

            var caseStatusId = await sender.Send(command);

            return TypedResults.Created($"/api/BasicInfo/CaseStatus/{caseStatusId}");
        }

        // Handler for updating an existing CaseStatus
        public async Task<IResult> UpdateCaseStatus([FromBody] UpdateCaseStatusCommand command, ISender sender)
        {
            if (string.IsNullOrEmpty(command.Name))
            {
                return TypedResults.BadRequest("The Name field is required.");
            }

            var updatedCaseStatusId = await sender.Send(command);

            if (updatedCaseStatusId == 0)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new CaseStatusDto { Id = updatedCaseStatusId, Name = command.Name });
        }

        // Handler for deleting a CaseStatus
        public static async Task<IResult> DeleteCaseStatus([FromBody] DeleteCaseStatusCommand command, ISender sender)
        {
            var result = await sender.Send(command);

            if (!result)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new { Message = "CaseStatus deleted successfully" });
        }

        // Handler for fetching all PredefinedSubjects
        public async Task<Results<Ok<List<PredefinedSubjectDto>>, NotFound>> GetAllCasePredefinedSubjects(ISender sender)
        {
            var PredefinedSubjects = await sender.Send(new GetAllCasePredefinedSubjectsQuery());

            /*if (PredefinedSubjects == null || !PredefinedSubjects.Any())
            {
                return TypedResults.NotFound();
            }
            */

            return TypedResults.Ok(PredefinedSubjects);
        }

        // Handler for creating a new Case Predefined Subject
        public static async Task<IResult> CreateCasePredefinedSubject(CreateCasePredefinedSubjectCommand command, ISender sender)
        {
            if (string.IsNullOrEmpty(command.Title))
            {
                return TypedResults.BadRequest("Title is required");
            }

            var casePredefinedSubjectId = await sender.Send(command);

            return TypedResults.Created($"/api/BasicInfo/CasePredefinedSubject/{casePredefinedSubjectId}");
        }

        // Handler for updating an existing Case Predefined Subject
        public async Task<IResult> UpdateCasePredefinedSubject([FromBody] UpdateCasePredefinedSubjectCommand command, ISender sender)
        {
            if (string.IsNullOrEmpty(command.Title))
            {
                return TypedResults.BadRequest("Name is required");
            }

            var updatedCasePredefinedSubjectId = await sender.Send(command);

            if (updatedCasePredefinedSubjectId == 0)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new PredefinedSubjectDto { Id = updatedCasePredefinedSubjectId, Title = command.Title, CaseTypeId = command.CaseTypeId });
        }

        // Handler for deleting a CasePredefinedSubject
        public static async Task<IResult> DeleteCasePredefinedSubject([FromBody] DeleteCasePredefinedSubjectCommand command, ISender sender)
        {
            var result = await sender.Send(command);

            if (!result)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new { Message = "CasePredefinedSubject deleted successfully" });
        }

        // Handler for fetching all Client Roles In Case
        public async Task<Results<Ok<List<ClientRoleInCaseDto>>, NotFound>> GetAllClientRolesInCase(ISender sender)
        {
            var ClientRolesInCase = await sender.Send(new GetAllClientRolesInCaseQuery());

            //if (ClientRolesInCase == null || !ClientRolesInCase.Any())
            //{
            //    return TypedResults.NotFound();
            //}

            return TypedResults.Ok(ClientRolesInCase);
        }

        // Handler for creating a new Client Role In Case
        public static async Task<IResult> CreateClientRoleInCase(CreateClientRoleInCaseCommand command, ISender sender)
        {
            if (string.IsNullOrEmpty(command.Title))
            {
                return TypedResults.BadRequest("Title is required");
            }

            var clientRoleInCaseId = await sender.Send(command);

            return TypedResults.Created($"/api/BasicInfo/ClientRoleInCase/{clientRoleInCaseId}");
        }

        // Handler for updating an existing Client Role In Case
        public async Task<IResult> UpdateClientRoleInCase([FromBody] UpdateClientRoleInCaseCommand command, ISender sender)
        {
            if (string.IsNullOrEmpty(command.Title))
            {
                return TypedResults.BadRequest("Name is required");
            }

            var updatedClientRoleInCaseId = await sender.Send(command);

            if (updatedClientRoleInCaseId == 0)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new ClientRoleInCaseDto { Id = updatedClientRoleInCaseId, Title = command.Title, CaseTypeId = command.CaseTypeId });
        }

        // Handler for deleting a ClientRoleInCase
        public static async Task<IResult> DeleteClientRoleInCase([FromBody] DeleteClientRoleInCaseCommand command, ISender sender)
        {
            var result = await sender.Send(command);

            if (!result)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new { Message = "ClientRoleInCase deleted successfully" });
        }

        // Handler for fetching all Judges
        public async Task<Results<Ok<List<JudgeDto>>, NotFound>> GetAllJudges(ISender sender)
        {
            var judges = await sender.Send(new GetAllJudgesQuery());

            /*if (judges == null || !judges.Any())
            {
                return TypedResults.NotFound();
            }
            */
            return TypedResults.Ok(judges);
        }

        // Handler for creating a new Judge
        public static async Task<IResult> CreateJudge(CreateJudgeCommand command, ISender sender)
        {
            if (string.IsNullOrEmpty(command.FirstName) || string.IsNullOrEmpty(command.LastName))
            {
                return TypedResults.BadRequest("Firstname and lastname are required");
            }

            var judgeId = await sender.Send(command);

            return TypedResults.Created($"/api/BasicInfo/Judge/{judgeId}");
        }

        // Handler for updating an existing Judge
        public async Task<IResult> UpdateJudge([FromBody] UpdateJudgeCommand command, ISender sender)
        {
            if (string.IsNullOrEmpty(command.FirstName) || string.IsNullOrEmpty(command.LastName))
            {
                return TypedResults.BadRequest("First name and last name are required");
            }

            var updatedJudgeId = await sender.Send(command);

            if (updatedJudgeId == 0)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new JudgeDto { Id = updatedJudgeId.ToString(), FirstName = command.FirstName, LastName = command.LastName });
        }

        // Handler for deleting a Judge
        public static async Task<IResult> DeleteJudge([FromBody] DeleteJudgeCommand command, ISender sender)
        {
            var result = await sender.Send(command);

            if (!result)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new { Message = "Judge deleted successfully" });
        }

        // Handler for fetching all CourtTypes
        public async Task<Results<Ok<List<CourtTypeDto>>, NotFound>> GetAllCourtTypes(ISender sender)
        {
            var courtTypes = await sender.Send(new GetAllCourtTypesQuery());

            //if (courtTypes == null || !courtTypes.Any())
            //{
            //    return TypedResults.NotFound();
            //}

            return TypedResults.Ok(courtTypes);
        }

        // Handler for creating a new CourtType
        public static async Task<IResult> CreateCourtType(CreateCourtTypeCommand command, ISender sender)
        {
            if (string.IsNullOrEmpty(command.Title))
            {
                return TypedResults.BadRequest("Title is required");
            }

            var courtTypeId = await sender.Send(command);

            return TypedResults.Created($"/api/BasicInfo/CourtType/{courtTypeId}");
        }

        // Handler for updating an existing CourtType
        public async Task<IResult> UpdateCourtType([FromBody] UpdateCourtTypeCommand command, ISender sender)
        {
            if (string.IsNullOrEmpty(command.Title))
            {
                return TypedResults.BadRequest("Title is required");
            }

            var updatedCourtTypeId = await sender.Send(command);

            if (updatedCourtTypeId == 0)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new CourtTypeDto { Id = updatedCourtTypeId, Title = command.Title });
        }

        // Handler for deleting a CourtType
        public static async Task<IResult> DeleteCourtType([FromBody] DeleteCourtTypeCommand command, ISender sender)
        {
            var result = await sender.Send(command);

            if (!result)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new { Message = "CourtType deleted successfully" });
        }

        // Handler for fetching all CourtSubTypes
        public async Task<Results<Ok<List<CourtSubtypeDto>>, NotFound>> GetAllCourtSubTypes(ISender sender)
        {
            var CourtSubTypes = await sender.Send(new GetAllCourtSubTypesQuery());

            //if (CourtSubTypes == null || !CourtSubTypes.Any())
            //{
            //    return TypedResults.NotFound();
            //}

            return TypedResults.Ok(CourtSubTypes);
        }

        // Handler for creating a new Court SubType
        public static async Task<IResult> CreateCourtSubType(CreateCourtSubTypeCommand command, ISender sender)
        {
            if (string.IsNullOrEmpty(command.Title))
            {
                return TypedResults.BadRequest("Title is required");
            }

            var courtSubTypeId = await sender.Send(command);

            return TypedResults.Created($"/api/BasicInfo/CourtSubType/{courtSubTypeId}");
        }

        // Handler for updating an existing Court SubType
        public async Task<IResult> UpdateCourtSubType([FromBody] UpdateCourtSubTypeCommand command, ISender sender)
        {
            if (string.IsNullOrEmpty(command.Title))
            {
                return TypedResults.BadRequest("Name is required");
            }

            var updatedCourtSubTypeId = await sender.Send(command);

            if (updatedCourtSubTypeId == 0)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new CourtSubtypeDto { Id = Convert.ToString(updatedCourtSubTypeId), Title = command.Title, CourtTypeId = command.CourtTypeId });
        }

        // Handler for deleting a CourtSubType
        public static async Task<IResult> DeleteCourtSubType([FromBody] DeleteCourtSubTypeCommand command, ISender sender)
        {
            var result = await sender.Send(command);


            if (!result)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new { Message = "CourtSubType deleted successfully" });
        }

    }
}
