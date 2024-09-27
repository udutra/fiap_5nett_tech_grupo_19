
using System.Text.Json.Serialization;
using fiap_5nett_tech.Domain.Entities;

namespace fiap_5nett_tech.Application.DataTransfer.Response
{
    public class RegionResponse
    {
        private readonly int _code;
        public Region? Data { get; set; }
        public string? Message { get; set; }

        [JsonIgnore]
        public bool IsSuccess => _code is >= 200 and <= 299;
    
        [JsonConstructor]
        public RegionResponse() =>  _code = Configuration.DefaultStatusCode;

        public RegionResponse(Region? data, int code = Configuration.DefaultStatusCode, string? message = null)
        {
            _code = code;
            Data = data;
            Message = message;
        }

    }
}
