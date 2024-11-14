using Microsoft.EntityFrameworkCore;
using RSA_UI.Models.Entity;
using RSA_UI.Repositories.Context;

namespace RSA_UI.Repositories.RSA;

public class CompanyRepository : IRepository<Company>
{
    private readonly RsaContext _context;
    public CompanyRepository(RsaContext context) => _context = context;

    public Company? GetById(string id)
    {
        return _context.Companies.FirstOrDefault(c => c.Id == id);
    }

    public bool Update(Company entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        return _context.SaveChanges() > 0;
    }

    public Company? Add(Company entity)
    {
        _context.Companies.Add(entity);
        return _context.SaveChanges() > 0 ? entity : null;
    }

    public bool Delete(Company entity)
    {
        var tracking = _context.Companies.Remove(entity);
        _context.SaveChanges();
        return tracking.State == EntityState.Deleted;
    }

    public Company? First(Func<Company, bool> f)
    {
        return _context.Companies.FirstOrDefault(f);
    }

    public IEnumerable<Company> GetAll()
    {
        return _context.Companies;
    }

    public IEnumerable<Company> Filter(Func<Company, bool> f)
    {
        return _context.Companies.Where(f);
    }
}