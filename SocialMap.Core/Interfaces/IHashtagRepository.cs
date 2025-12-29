using SocialMap.Core.Entities;

namespace SocialMap.Core.Interfaces;

public interface IHashtagRepository : IRepository<Hashtag>
{
    Task<Hashtag?> GetByNameAsync(string name);
    Task<IEnumerable<Hashtag>> GetTrendingAsync(int count = 10);
    Task<IEnumerable<Hashtag>> SearchAsync(string query, int limit = 20);
    Task<IEnumerable<Post>> GetPostsByHashtagAsync(string hashtagName, int page = 1, int pageSize = 20);
}
