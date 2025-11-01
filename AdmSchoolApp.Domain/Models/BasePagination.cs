using System.Text.Json.Serialization;

namespace AdmSchoolApp.Domain.Models;

public class BasePagination<T>
{
    public BasePagination()
    {
        Items = [];
    }

    public BasePagination(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
    
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    
    [JsonPropertyName("items")]
    public IReadOnlyList<T> Items { get; set; }
}