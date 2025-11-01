using System.Text.Json;
using System.Text.Json.Serialization;
using AdmSchoolApp.Domain.Models.Responses;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace AdmSchoolApp.Extensions;

public static class HealthCheckEndpoints
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    public static IEndpointRouteBuilder MapAppHealthChecks(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health");

        app.MapHealthChecks("/healthz", new HealthCheckOptions
        {
            ResponseWriter = async (ctx, report) =>
            {
                ctx.Response.ContentType = "application/json";

                var dto = new HealthReportResult
                {
                    Status = report.Status.ToString(),
                    Results = report.Entries.Select(e => new HealthReportEntryDto
                    {
                        Name = e.Key,
                        Status = e.Value.Status.ToString(),
                        Duration = e.Value.Duration.TotalMilliseconds,
                        Error = e.Value.Exception?.Message
                    }).ToList()
                };

                await ctx.Response.WriteAsJsonAsync(dto, Options);
            }
        });

        return app;
    }
}