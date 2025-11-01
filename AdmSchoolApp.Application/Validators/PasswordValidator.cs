using System.Text.RegularExpressions;
using FluentValidation;

namespace AdmSchoolApp.Application.Validators;

public class PasswordValidator : AbstractValidator<string>
{
    public PasswordValidator()
    {
        // REQUISITO 7: Senha forte com no mínimo 8 caracteres, maiúsculas, minúsculas, números e símbolos
        RuleFor(password => password)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres")
            .Must(HaveUpperCase).WithMessage("Senha deve conter pelo menos uma letra maiúscula")
            .Must(HaveLowerCase).WithMessage("Senha deve conter pelo menos uma letra minúscula")
            .Must(HaveDigit).WithMessage("Senha deve conter pelo menos um número")
            .Must(HaveSpecialChar).WithMessage("Senha deve conter pelo menos um caractere especial");
    }

    private static bool HaveUpperCase(string password) => 
        !string.IsNullOrEmpty(password) && password.Any(char.IsUpper);

    private static bool HaveLowerCase(string password) => 
        !string.IsNullOrEmpty(password) && password.Any(char.IsLower);

    private static bool HaveDigit(string password) => 
        !string.IsNullOrEmpty(password) && password.Any(char.IsDigit);

    private static bool HaveSpecialChar(string password) => 
        !string.IsNullOrEmpty(password) && Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>]");
}