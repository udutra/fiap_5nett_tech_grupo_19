using fiap_5nett_tech.Domain.Entities;

namespace fiap_5nett_tech.Domain.Repositories;

public interface IRegionRepository
{
    IQueryable<Region> GetAll(string name);
    Region? GetOne(int id);
}