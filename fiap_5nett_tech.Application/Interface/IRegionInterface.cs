using fiap_5nett_tech.Application.DataTransfer.Request;
using fiap_5nett_tech.Application.DataTransfer.Response;

namespace fiap_5nett_tech.Application.Interface;

public interface IRegionInterface
{
    RegionResponse GetOne(int id);
    List<RegionResponse> GetAll(string name);
}