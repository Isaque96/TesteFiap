using System.Text.Json.Serialization;

namespace AdmSchoolApp.Domain.Models.Responses;

public sealed class HealthReportResult
{
    public string? Status { get; set; } = null;
    public List<HealthReportEntryDto> Results { get; set; } = [];
}

public sealed class HealthReportEntryDto
{
    public string? Name { get; set; } = null;
    public string? Status { get; set; } = null;
    public double Duration { get; set; }
    public string? Error { get; set; }
}

[JsonSerializable(typeof(HealthReportResult))]
[JsonSourceGenerationOptions(WriteIndented = false)]
public partial class AppJsonContext : JsonSerializerContext { }