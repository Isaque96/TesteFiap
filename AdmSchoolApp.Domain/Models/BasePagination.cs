using System.Text.Json.Serialization;

namespace AdmSchoolApp.Domain.Models;

public class BasePagination<T>(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
{
    [JsonPropertyName("page")]
    public int Page { get; set; } = page;

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; } = pageSize;

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; } = totalCount;

    [JsonPropertyName("totalPages")]
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    
    [JsonPropertyName("items")]
    public IReadOnlyList<T> Items { get; set; } = items;
}