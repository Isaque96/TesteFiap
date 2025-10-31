using System.Text.Json.Serialization;

namespace AdmSchoolApp.Domain.Models;

public sealed class HealthReportDto
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

[JsonSerializable(typeof(HealthReportDto))]
[JsonSourceGenerationOptions(WriteIndented = false)]
public partial class AppJsonContext : JsonSerializerContext { }