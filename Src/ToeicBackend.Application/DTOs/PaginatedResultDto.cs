namespace ToeicBackend.Application.DTOs;

public class PaginatedResultDto<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
}
