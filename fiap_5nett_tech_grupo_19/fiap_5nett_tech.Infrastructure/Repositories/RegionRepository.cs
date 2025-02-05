using fiap_5nett_tech.Domain.Entities;
using fiap_5nett_tech.Domain.Repositories;
using fiap_5nett_tech.Infrastructure.Data;

namespace fiap_5nett_tech.Infrastructure.Repositories;

public class RegionRepository : IRegionRepository
{
    private readonly AppDbContext _context;
    
    public RegionRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public IQueryable<Region> GetAll(string name)
    {
        return _context.Regions.Where(x => x.Name == name);
    }

    public Region? GetOne(int id)
    {
        return _context.Regions.FirstOrDefault(x => x.Ddd == id);
    }
}