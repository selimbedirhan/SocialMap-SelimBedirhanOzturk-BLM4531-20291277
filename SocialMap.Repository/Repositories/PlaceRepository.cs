using Microsoft.EntityFrameworkCore;
using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;
using SocialMap.Repository.Data;

namespace SocialMap.Repository.Repositories;

public class PlaceRepository : Repository<Place>, IPlaceRepository
{
    public PlaceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public new async Task<Place?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.CreatedBy)
            .Include(p => p.Posts)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public new async Task<IEnumerable<Place>> GetAllAsync()
    {
        return await _dbSet
            .Include(p => p.CreatedBy)
            .ToListAsync();
    }

    public async Task<IEnumerable<Place>> GetByCityAsync(string city)
    {
        return await _dbSet
            .Include(p => p.CreatedBy)
            .Where(p => p.City == city)
            .ToListAsync();
    }

    public async Task<IEnumerable<Place>> SearchByNameAsync(string name)
    {
        return await _dbSet
            .Include(p => p.CreatedBy)
            .Where(p => p.Name.Contains(name))
            .ToListAsync();
    }

    public async Task<IEnumerable<Place>> GetByCreatedByIdAsync(Guid createdById)
    {
        return await _dbSet
            .Include(p => p.CreatedBy)
            .Where(p => p.CreatedById == createdById)
            .ToListAsync();
    }

    public async Task<IEnumerable<Place>> GetWithinBoundsAsync(double minLatitude, double maxLatitude, double minLongitude, double maxLongitude)
    {
        return await _dbSet
            .Include(p => p.CreatedBy)
            .Include(p => p.Posts)
            .Where(p =>
                p.Latitude.HasValue && p.Longitude.HasValue &&
                p.Latitude.Value >= minLatitude &&
                p.Latitude.Value <= maxLatitude &&
                p.Longitude.Value >= minLongitude &&
                p.Longitude.Value <= maxLongitude)
            .ToListAsync();
    }
}

