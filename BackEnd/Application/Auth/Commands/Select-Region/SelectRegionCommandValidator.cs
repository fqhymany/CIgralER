

namespace LawyerProject.Application.Auth.Commands.Select_Region;

public class SelectRegionCommandValidator : AbstractValidator<SelectRegionCommand>
{
    public SelectRegionCommandValidator()
    {
        RuleFor(v => v.UserId)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(v => v.RegionId.ToString())
            .NotEmpty()
            .MaximumLength(100);
    }
}
