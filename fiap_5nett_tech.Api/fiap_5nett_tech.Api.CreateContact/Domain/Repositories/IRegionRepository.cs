using fiap_5nett_tech.Api.CreateContact.Domain.Entities;

namespace fiap_5nett_tech.Api.CreateContact.Domain.Repositories;

public interface IRegionRepository
{
    IQueryable<Region> GetAll(string name);
    Region? GetOne(int id);
}