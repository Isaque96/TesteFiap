using AdmSchoolApp.Domain.Enums;
using AdmSchoolApp.Domain.Models;
using FluentValidation.Results;

namespace AdmSchoolApp.Extensions;

public static class ApiResponseExtensions
{
    public static IResult Success<T>(T data, string message = "Operação realizada com sucesso")
    {
        var response = new BaseResponse<T>(message, data);
        return Results.Ok(response);
    }

    public static IResult Created<T>(T data, string uri, string message = "Recurso criado com sucesso")
    {
        var response = new BaseResponse<T>(message, data);
        return Results.Created(uri, response);
    }

    public static IResult BadRequest(string[] errors, InternalCodes code = InternalCodes.MalformedRequest)
    {
        var response = new BaseResponse<object>(
            message: "Requisição inválida",
            data: null,
            error: new BaseError(errors, code)
        );
        return Results.BadRequest(response);
    }

    public static IResult BadRequest(ValidationResult validationResult)
    {
        var errors = validationResult.Errors
            .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
            .ToArray();
        
        return BadRequest(errors);
    }

    public static IResult Unauthorized(string message = "Não autorizado")
    {
        var response = new BaseResponse<object>(
            message: message,
            data: null,
            error: new BaseError(
                ["Credenciais inválidas ou token expirado"],
                InternalCodes.UnauthorizedRequest
            )
        );
        return Results.Unauthorized();
    }

    public static IResult NotFound(string message = "Recurso não encontrado")
    {
        var response = new BaseResponse<object>(
            message: message,
            data: null,
            error: new BaseError(
                [message],
                InternalCodes.MalformedRequest
            )
        );
        return Results.NotFound(response);
    }

    public static IResult InternalError(string message = "Erro interno do servidor")
    {
        var response = new BaseResponse<object>(
            message: message,
            data: null,
            error: new BaseError(
                ["Ocorreu um erro inesperado. Tente novamente mais tarde."],
                InternalCodes.InternalError
            )
        );
        return Results.InternalServerError(response);
    }

    public static IResult NoContent(string message = "Operação realizada com sucesso")
    {
        var response = new BaseResponse<object>(message, null);
        return Results.Ok(response);
    }
}