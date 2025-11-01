using System.Text.Json.Serialization;

namespace AdmSchoolApp.Domain.Models.Responses;

public sealed class HealthReportResult
{
    public string? Status { get; set; }
    public List<HealthReportEntryDto> Results { get; set; } = [];
}

public sealed class HealthReportEntryDto
{
    public string? Name { get; set; }
    public string? Status { get; set; }
    public double Duration { get; set; }
    public string? Error { get; set; }
}