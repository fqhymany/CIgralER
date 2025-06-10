namespace LawyerProject.Application.UserRoles.Queries.GetUserNamesByRoleName;

public class GetUserNamesByRoleNameValidator : AbstractValidator<GetUserNamesByRoleNameCommand>
{
    public GetUserNamesByRoleNameValidator()
    {
        RuleFor(v => v.RoleName)
            .MaximumLength(200)
            .NotEmpty();
    }
}
