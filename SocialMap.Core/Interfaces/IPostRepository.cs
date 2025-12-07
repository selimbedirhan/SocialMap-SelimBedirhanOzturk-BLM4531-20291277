using SocialMap.Core.Entities;

namespace SocialMap.Core.Interfaces;

public interface IPostRepository : IRepository<Post>
{
    Task<IEnumerable<Post>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Post>> GetByPlaceIdAsync(Guid placeId);
    Task<IEnumerable<Post>> GetRecentPostsAsync(int count);
    Task<IEnumerable<Post>> GetPostsWithinBoundsAsync(double minLatitude, double maxLatitude, double minLongitude, double maxLongitude);
    Task<IEnumerable<Post>> GetPostsByGeohashPrefixAsync(string geohashPrefix);
}

