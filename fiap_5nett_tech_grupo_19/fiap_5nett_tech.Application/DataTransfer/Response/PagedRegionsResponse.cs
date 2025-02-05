using System.Text.Json.Serialization;
using fiap_5nett_tech.Domain.Entities;

namespace fiap_5nett_tech.Application.DataTransfer.Response;

public class PagedRegionsResponse : RegionResponse
{
    public int CurrentPage { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double) PageSize);
    public int PageSize { get; set; } = Configuration.DefaultPageSize;
    public int TotalCount { get; set; }


    [JsonConstructor]
    public PagedRegionsResponse(
        Region? data,
        int totalCount,
        int currentPage = Configuration.DefaultCurrentPage,
        int pageSize = Configuration.DefaultPageSize)
        : base(data)
    {
        Data = data;
        TotalCount = totalCount;
        CurrentPage = currentPage;
        PageSize = pageSize;
    }

    public PagedRegionsResponse(
        Region? data,
        int code = Configuration.DefaultStatusCode,
        string? message = null)
        : base(data, code, message)
    {
    }
}