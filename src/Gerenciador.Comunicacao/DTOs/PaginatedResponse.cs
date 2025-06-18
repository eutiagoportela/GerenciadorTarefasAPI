namespace Gerenciador.Comunicacao.DTOs;

public class PaginatedResponse<T>
{
    public List<T> Data { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public string? SearchTerm { get; set; }

    public static PaginatedResponse<T> Create(
        List<T> data,
        int pageNumber,
        int pageSize,
        int totalCount,
        string? searchTerm = null)
    {
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PaginatedResponse<T>
        {
            Data = data,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = pageNumber < totalPages,
            HasPreviousPage = pageNumber > 1,
            SearchTerm = searchTerm
        };
    }
}