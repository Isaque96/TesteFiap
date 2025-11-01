using AdmSchoolApp.Domain.Entities;
using FluentValidation;

namespace AdmSchoolApp.Application.Validators;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido")
            .MaximumLength(256).WithMessage("Email deve ter no máximo 256 caracteres");
    }
}