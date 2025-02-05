namespace fiap_5nett_tech.Application.DataTransfer.Request;

public class GetAllContactRequest : ContactRequestAll
{
    public int PageNumber { get; set; } = Configuration.DefaultPageNumber;
    public int PageSize { get; set; } = Configuration.DefaultPageSize;
}