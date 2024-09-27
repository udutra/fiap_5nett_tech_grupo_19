using System.Text.Json.Serialization;

namespace fiap_5nett_tech.Application.DataTransfer.Response
{
    public class ContactResponse<TData>
    {
        public readonly int Code;
        public TData? Data { get; set; }
        public string? Message { get; set; }

        [JsonIgnore]
        public bool IsSuccess => Code is >= 200 and <= 299;
    
        [JsonConstructor]
        public ContactResponse() =>  Code = Configuration.DefaultStatusCode;

        public ContactResponse(TData? data, int code = Configuration.DefaultStatusCode, string? message = null)
        {
            Code = code;
            Data = data;
            Message = message;
        }
    }
}
