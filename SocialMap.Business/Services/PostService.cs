using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;
using SocialMap.Business.Utils;

namespace SocialMap.Business.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPlaceRepository _placeRepository;
    private readonly IFollowRepository _followRepository;
    private readonly INotificationService _notificationService;
    private readonly ICacheService _cacheService;

    public PostService(
        IPostRepository postRepository, 
        IUserRepository userRepository, 
        IPlaceRepository placeRepository, 
        IFollowRepository followRepository, 
        INotificationService notificationService,
        ICacheService cacheService)
    {
        _postRepository = postRepository;
        _userRepository = userRepository;
        _placeRepository = placeRepository;
        _followRepository = followRepository;
        _notificationService = notificationService;
        _cacheService = cacheService;
    }

    public async Task<Post?> GetPostByIdAsync(Guid id)
    {
        return await _postRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Post>> GetPostsByUserIdAsync(Guid userId)
    {
        return await _postRepository.GetByUserIdAsync(userId);
    }

    public async Task<IEnumerable<Post>> GetPostsByPlaceIdAsync(Guid placeId)
    {
        return await _postRepository.GetByPlaceIdAsync(placeId);
    }

    public async Task<IEnumerable<Post>> GetRecentPostsAsync(int count = 20)
    {
        string cacheKey = $"recent_posts_{count}";
        var cachedPosts = await _cacheService.GetAsync<IEnumerable<Post>>(cacheKey);
        
        if (cachedPosts != null)
        {
            return cachedPosts;
        }

        var posts = await _postRepository.GetRecentPostsAsync(count);
        await _cacheService.SetAsync(cacheKey, posts, TimeSpan.FromMinutes(10));
        
        return posts;
    }

    public async Task<(IEnumerable<Post> Items, int TotalCount)> GetRecentPostsPagedAsync(int page, int pageSize)
    {
        string cacheKey = $"recent_posts_paged_{page}_{pageSize}";
        var cached = await _cacheService.GetAsync<(IEnumerable<Post>, int)>(cacheKey);
        
        if (cached != default)
        {
            return cached;
        }

        var result = await _postRepository.GetRecentPostsPagedAsync(page, pageSize);
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
        
        return result;
    }

    public async Task<Post> CreatePostAsync(Guid userId, Guid? placeId, string? placeName, double? latitude, double? longitude, string? city, string? country, string? mediaUrl, string? caption)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException($"User with ID '{userId}' not found.");

        // Yer etiketi zorunlu - ya placeId ya da placeName + koordinatlar olmalı
        if (!placeId.HasValue && (string.IsNullOrWhiteSpace(placeName) || !latitude.HasValue || !longitude.HasValue))
        {
            throw new InvalidOperationException("Yer etiketi zorunludur. Lütfen bir yer seçin veya harita üzerinden konum seçin.");
        }

        Place? place = null;
        string? finalPlaceName = placeName;
        string? finalCity = city;
        string? finalCountry = country;
        double? finalLatitude = latitude;
        double? finalLongitude = longitude;

        // Eğer placeId verilmişse, veritabanından Place'i al (geriye dönük uyumluluk)
        if (placeId.HasValue && placeId.Value != Guid.Empty)
        {
            place = await _placeRepository.GetByIdAsync(placeId.Value);
            if (place == null)
                throw new InvalidOperationException($"Place with ID '{placeId}' not found.");

            // Place'den bilgileri al, ama yeni veriler varsa onları kullan
            if (string.IsNullOrWhiteSpace(finalPlaceName))
                finalPlaceName = place.Name;
            if (string.IsNullOrWhiteSpace(finalCity))
                finalCity = place.City;
            if (string.IsNullOrWhiteSpace(finalCountry))
                finalCountry = place.Country;
            if (!finalLatitude.HasValue)
                finalLatitude = place.Latitude;
            if (!finalLongitude.HasValue)
                finalLongitude = place.Longitude;
        }

        // Koordinatlar zorunlu
        if (!finalLatitude.HasValue || !finalLongitude.HasValue)
        {
            throw new InvalidOperationException("Yer koordinatları zorunludur. Lütfen harita üzerinden bir konum seçin.");
        }

        var geohash = GeohashUtil.Encode(finalLatitude.Value, finalLongitude.Value);

        var post = new Post
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlaceId = placeId,
            Place = place,
            PlaceName = finalPlaceName,
            City = finalCity,
            Country = finalCountry ?? "Türkiye",
            MediaUrl = mediaUrl,
            Caption = caption,
            Latitude = finalLatitude,
            Longitude = finalLongitude,
            Geohash = geohash,
            LikesCount = 0,
            CommentsCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        var addedPost = await _postRepository.AddAsync(post);
        await _cacheService.RemoveAsync("recent_posts_20");
        await _cacheService.RemoveAsync("recent_posts_100");

        // Takip edenlere bildirim gönder
        var followers = await _followRepository.GetFollowersAsync(userId);
        foreach (var follow in followers)
        {
            await _notificationService.CreateNotificationAsync(
                follow.FollowerId,
                "post",
                $"{user.Username} yeni bir gönderi paylaştı",
                relatedPostId: addedPost.Id,
                relatedUserId: userId
            );
        }

        return addedPost;
    }

    public async Task UpdatePostAsync(Post post)
    {
        var existingPost = await _postRepository.GetByIdAsync(post.Id);
        if (existingPost == null)
            throw new InvalidOperationException($"Post with ID '{post.Id}' not found.");

        await _postRepository.UpdateAsync(post);
        await _cacheService.RemoveAsync("recent_posts_20");
        await _cacheService.RemoveAsync("recent_posts_100");
    }

    public async Task DeletePostAsync(Guid id)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post == null)
            throw new InvalidOperationException($"Post with ID '{id}' not found.");

        await _postRepository.DeleteAsync(post);
        await _cacheService.RemoveAsync("recent_posts_20");
        await _cacheService.RemoveAsync("recent_posts_100");
    }

    public async Task<IEnumerable<Post>> SearchPostsAsync(string? term, string? city, DateTime? fromDate, DateTime? toDate)
    {
        return await _postRepository.SearchPostsAsync(term, city, fromDate, toDate);
    }
}

