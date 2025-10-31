using System.Text.Json;
using AdmSchoolApp.Domain.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace AdmSchoolApp.Extensions;

public static class HealthCheckEndpoints
{
    public static IEndpointRouteBuilder MapAppHealthChecks(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health");

        app.MapHealthChecks("/healthz", new HealthCheckOptions
        {
            ResponseWriter = async (ctx, report) =>
            {
                ctx.Response.ContentType = "application/json";

                var dto = new HealthReportDto
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

                await ctx.Response.WriteAsync(
                    JsonSerializer.Serialize(dto, AppJsonContext.Default.HealthReportDto)
                );
            }
        });

        return app;
    }
}