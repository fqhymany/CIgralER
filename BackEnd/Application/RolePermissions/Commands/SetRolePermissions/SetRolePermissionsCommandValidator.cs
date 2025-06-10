using FluentValidation;

namespace LawyerProject.Application.RolePermissions.Commands.SetRolePermissions;

public class SetRolePermissionsCommandValidator : AbstractValidator<SetRolePermissionsCommand>
{
    public SetRolePermissionsCommandValidator()
    {
        RuleFor(v => v.RoleId)
            .NotEmpty().WithMessage("شناسه نقش نمی‌تواند خالی باشد");

        RuleFor(v => v.Permissions)
            .NotEmpty().WithMessage("لیست دسترسی‌ها نمی‌تواند خالی باشد");

        RuleForEach(v => v.Permissions)
            .ChildRules(permission =>
            {
                permission.RuleFor(p => p.Section)
                    .NotEmpty().WithMessage("نام بخش باید مشخص شود");
            });
    }
}
