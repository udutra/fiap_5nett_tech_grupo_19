using fiap_5nett_tech.Api.CreateContact.Application.DataTransfer.Response;

namespace fiap_5nett_tech.Api.CreateContact.Application.Interface;

public interface IRegionInterface
{
    RegionResponse GetOne(int id);
    List<RegionResponse> GetAll(string name);
}