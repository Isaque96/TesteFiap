using System.Text.RegularExpressions;
using AdmSchoolApp.Domain.Entities;
using FluentValidation;

namespace AdmSchoolApp.Application.Validators;

public partial class StudentValidator : AbstractValidator<Student>
{
    public StudentValidator()
    {
        // REQUISITO 3: Nome entre 3 e 100 caracteres
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .Length(3, 100).WithMessage("Nome deve ter entre 3 e 100 caracteres");

        // REQUISITO 4: CPF válido com 11 dígitos
        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("CPF é obrigatório")
            .Length(11).WithMessage("CPF deve ter 11 dígitos")
            .Must(BeValidCpf).WithMessage("CPF inválido");

        // REQUISITO 4: Email válido
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido")
            .MaximumLength(256).WithMessage("Email deve ter no máximo 256 caracteres");

        // REQUISITO 4: Data de nascimento válida
        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("Data de nascimento é obrigatória")
            .Must(BeValidBirthDate).WithMessage("Data de nascimento inválida (não pode ser futura ou muito antiga)");
    }

    private bool BeValidCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return false;

        // Remove caracteres não numéricos
        cpf = NumericRgx().Replace(cpf, "");

        if (cpf.Length != 11)
            return false;

        // Verifica se todos os dígitos são iguais
        if (cpf.Distinct().Count() == 1)
            return false;

        // Validação dos dígitos verificadores
        var multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        var multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        var tempCpf = cpf[..9];
        var soma = 0;

        for (var i = 0; i < 9; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

        var resto = soma % 11;
        resto = resto < 2 ? 0 : 11 - resto;

        var digito = resto.ToString();
        tempCpf += digito;
        soma = 0;

        for (var i = 0; i < 10; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

        resto = soma % 11;
        resto = resto < 2 ? 0 : 11 - resto;
        digito += resto.ToString();

        return cpf.EndsWith(digito);
    }

    private static bool BeValidBirthDate(DateOnly birthDate)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var minDate = today.AddYears(-120); // Não pode ser mais de 120 anos atrás
        
        return birthDate < today && birthDate > minDate;
    }

    [GeneratedRegex(@"[^\d]")]
    private static partial Regex NumericRgx();
}