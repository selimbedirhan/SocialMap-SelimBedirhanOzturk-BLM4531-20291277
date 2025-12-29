using Microsoft.EntityFrameworkCore;
using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;
using SocialMap.Repository.Data;

namespace SocialMap.Repository.Repositories;

public class PostRepository : Repository<Post>, IPostRepository
{
    public PostRepository(ApplicationDbContext context) : base(context)
    {
    }

    public new async Task<Post?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Place)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .Include(p => p.Likes)
                .ThenInclude(l => l.User)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public new async Task<IEnumerable<Post>> GetAllAsync()
    {
        return await _dbSet
            .Include(p => p.Place)
            .Include(p => p.Comments)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Place)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .Include(p => p.Likes)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetByPlaceIdAsync(Guid placeId)
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Place)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .Include(p => p.Likes)
            .Where(p => p.PlaceId == placeId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetRecentPostsAsync(int count)
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Place)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .Include(p => p.Likes)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Post> Items, int TotalCount)> GetRecentPostsPagedAsync(int page, int pageSize)
    {
        var query = _dbSet
            .Include(p => p.User)
            .Include(p => p.Place)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .Include(p => p.Likes)
            .OrderByDescending(p => p.CreatedAt);

        var totalCount = await _dbSet.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<Post>> GetPostsWithinBoundsAsync(double minLatitude, double maxLatitude, double minLongitude, double maxLongitude)
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Place)
            .Include(p => p.Comments)
            .Include(p => p.Likes)
            .Where(p => p.Latitude.HasValue && p.Longitude.HasValue &&
                       p.Latitude.Value >= minLatitude && p.Latitude.Value <= maxLatitude &&
                       p.Longitude.Value >= minLongitude && p.Longitude.Value <= maxLongitude)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetPostsByGeohashPrefixAsync(string geohashPrefix)
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Place)
            .Include(p => p.Comments)
            .Include(p => p.Likes)
            .Where(p => p.Geohash != null && p.Geohash.StartsWith(geohashPrefix))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> SearchPostsAsync(string? term, string? city, DateTime? fromDate, DateTime? toDate)
    {
        var query = _dbSet
            .Include(p => p.User)
            .Include(p => p.Place)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .Include(p => p.Likes)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(term))
        {
            var lowerTerm = term.ToLower();
            query = query.Where(p =>
                (p.Caption != null && p.Caption.ToLower().Contains(lowerTerm)) ||
                // Custom Location Data
                (p.PlaceName != null && p.PlaceName.ToLower().Contains(lowerTerm)) || 
                (p.City != null && p.City.ToLower().Contains(lowerTerm)) ||
                (p.Country != null && p.Country.ToLower().Contains(lowerTerm)) ||
                // Official Place Data
                (p.Place != null && (
                    p.Place.Name.ToLower().Contains(lowerTerm) ||
                    p.Place.City.ToLower().Contains(lowerTerm) ||
                    p.Place.District != null && p.Place.District.ToLower().Contains(lowerTerm) ||
                    p.Place.Country != null && p.Place.Country.ToLower().Contains(lowerTerm)
                ))
            );
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            var lowerCity = city.ToLower();
            // Critical Fix: Check BOTH Post.City AND Place.City
            query = query.Where(p =>
                (p.City != null && p.City.ToLower().Contains(lowerCity)) ||
                (p.Place != null && p.Place.City.ToLower().Contains(lowerCity))
            );
        }

        if (fromDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= toDate.Value);
        }

        return await query.OrderByDescending(p => p.CreatedAt).Take(100).ToListAsync();
    }
}

