using AdmSchoolApp.Application.Utils;
using AdmSchoolApp.Domain.Enums;
using AdmSchoolApp.Domain.Models;
using Microsoft.AspNetCore.Diagnostics;

namespace AdmSchoolApp.Extensions;

public static class ErrorHandlingEndpoints
{
    public static IEndpointRouteBuilder MapErrorEndpoint(this IEndpointRouteBuilder app)
    {
        app.Map("/error", (HttpContext http) =>
        {
            var isProduction = !string.Equals(
                http.RequestServices.GetRequiredService<IHostEnvironment>().EnvironmentName,
                Environments.Development,
                StringComparison.OrdinalIgnoreCase
            );

            var exception = http.Features.Get<IExceptionHandlerFeature>()?.Error;

            var error = new BaseResponse<string>
            {
                Message = "Ocorreu um erro inesperado",
                Data = isProduction ? null : exception.ToString(),
                Error = isProduction
                    ? null
                    : new BaseError(
                        exception.GetAllExceptions().Select(e => e.Message).ToArray(),
                        InternalCodes.InternalError
                      )
            };

            Serilog.Log.Error(exception, "[{Id}] Erro não tratado", error.Error.TraceId);

            return Results.InternalServerError(error);
        });

        return app;
    }
}