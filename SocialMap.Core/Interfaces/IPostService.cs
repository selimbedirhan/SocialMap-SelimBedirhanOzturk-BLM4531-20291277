using SocialMap.Core.Entities;

namespace SocialMap.Core.Interfaces;

public interface IPostService
{
    Task<Post?> GetPostByIdAsync(Guid id);
    Task<IEnumerable<Post>> GetPostsByUserIdAsync(Guid userId);
    Task<IEnumerable<Post>> GetPostsByPlaceIdAsync(Guid placeId);
    Task<IEnumerable<Post>> GetRecentPostsAsync(int count = 20);
    Task<Post> CreatePostAsync(Guid userId, Guid? placeId, string? placeName, double? latitude, double? longitude, string? city, string? country, string? mediaUrl, string? caption);
    Task UpdatePostAsync(Post post);
    Task DeletePostAsync(Guid id);
}

