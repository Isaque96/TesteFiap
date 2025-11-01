using AdmSchoolApp.Domain.Entities;
using FluentValidation;

namespace AdmSchoolApp.Application.Validators;

public class ClassValidator : AbstractValidator<Class>
{
    public ClassValidator()
    {
        // REQUISITO 3: Nome entre 3 e 100 caracteres
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome da turma é obrigatório")
            .Length(3, 100).WithMessage("Nome da turma deve ter entre 3 e 100 caracteres");

        // REQUISITO 3: Descrição entre 10 e 250 caracteres
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Descrição da turma é obrigatória")
            .Length(10, 250).WithMessage("Descrição da turma deve ter entre 10 e 250 caracteres");
    }
}
