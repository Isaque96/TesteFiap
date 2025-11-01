using AdmSchoolApp.Domain.Entities;
using FluentValidation;

namespace AdmSchoolApp.Application.Validators;

public class EnrollmentValidator : AbstractValidator<Enrollment>
{
    public EnrollmentValidator()
    {
        // REQUISITO 4: Campos obrigatórios
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("ID do aluno é obrigatório");

        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("ID da turma é obrigatório");
    }
}