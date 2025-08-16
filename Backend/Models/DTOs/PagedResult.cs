using System.Text.Json.Serialization;

namespace FlowerSellingWebsite.Models.DTOs
{
    public sealed class PagedResult<T>
    {
        [JsonPropertyName("page")]
        public int Page { get; init; }

        [JsonPropertyName("page_size")]
        public int PageSize { get; init; }

        [JsonPropertyName("total_items")]
        public long TotalItems { get; init; }

        [JsonPropertyName("total_pages")]
        public int TotalPages { get; init; }

        [JsonPropertyName("has_next")]
        public bool HasNext => Page < TotalPages;

        [JsonPropertyName("has_prev")]
        public bool HasPrev => Page > 1;

        [JsonPropertyName("items")]
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    }

    public enum SortDirection { Asc, Desc }

    public class UrlQueryParams
    {
        [JsonPropertyName("page_index")]
        public int Page { get; set; }

        [JsonPropertyName("page_size")]
        public int PageSize { get; set; }

        [JsonPropertyName("sort_by")]
        public string? SortBy { get; set; }

        [JsonPropertyName("sort_direction")]
        public SortDirection? SortDirection { get; set; }

        [JsonPropertyName("search_by")]
        public string? SearchBy { get; set; }

        [JsonPropertyName("search_value")]
        public string? SearchValue { get; set; }

        [JsonPropertyName("filers")]
        public Dictionary<string, string>? FilterParams { get; set; }
    }
}
