using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;
using SocialMap.Business.Utils;

namespace SocialMap.Business.Services;

/// <summary>
/// Mevcut postların geohash ve koordinat bilgilerini günceller.
/// Place'den koordinatları alıp post'a kopyalar ve geohash hesaplar.
/// </summary>
public class PostGeohashUpdateService
{
    private readonly IPostRepository _postRepository;
    private readonly IPlaceRepository _placeRepository;

    public PostGeohashUpdateService(IPostRepository postRepository, IPlaceRepository placeRepository)
    {
        _postRepository = postRepository;
        _placeRepository = placeRepository;
    }

    /// <summary>
    /// Tüm postların geohash'lerini günceller.
    /// </summary>
    /// <returns>Güncellenen post sayısı</returns>
    public async Task<int> UpdateAllPostGeohashesAsync()
    {
        var allPosts = await _postRepository.GetAllAsync();
        var updatedCount = 0;

        foreach (var post in allPosts)
        {
            var updated = await UpdatePostGeohashAsync(post);
            if (updated)
                updatedCount++;
        }

        return updatedCount;
    }

    /// <summary>
    /// Belirli bir postun geohash'ini günceller.
    /// </summary>
    public async Task<bool> UpdatePostGeohashAsync(Post post)
    {
        double? latitude = null;
        double? longitude = null;

        // Önce post'taki direkt koordinatları kontrol et (yeni sistem)
        if (post.Latitude.HasValue && post.Longitude.HasValue)
        {
            latitude = post.Latitude.Value;
            longitude = post.Longitude.Value;
        }
        // Eğer post'ta koordinat yoksa ve PlaceId varsa, Place'den al (eski sistem - geriye dönük uyumluluk)
        else if (post.PlaceId.HasValue)
        {
            var place = await _placeRepository.GetByIdAsync(post.PlaceId.Value);
            if (place == null)
                return false;

            if (!place.Latitude.HasValue || !place.Longitude.HasValue)
                return false;

            latitude = place.Latitude.Value;
            longitude = place.Longitude.Value;
        }
        else
        {
            // Ne post'ta ne de Place'de koordinat yok
            return false;
        }

        // Geohash güncellemesi gerekli mi kontrol et
        var needsUpdate = false;

        if (!post.Latitude.HasValue || !post.Longitude.HasValue)
        {
            needsUpdate = true;
        }
        else if (Math.Abs(post.Latitude.Value - latitude.Value) > 0.0001 ||
                 Math.Abs(post.Longitude.Value - longitude.Value) > 0.0001)
        {
            needsUpdate = true;
        }

        if (string.IsNullOrEmpty(post.Geohash))
        {
            needsUpdate = true;
        }

        if (!needsUpdate)
            return false;

        // Koordinatları ve geohash'i güncelle
        post.Latitude = latitude.Value;
        post.Longitude = longitude.Value;
        post.Geohash = GeohashUtil.Encode(latitude.Value, longitude.Value);

        await _postRepository.UpdateAsync(post);
        return true;
    }

    /// <summary>
    /// CommentsCount'u günceller (post başına yorum sayısını hesaplar).
    /// </summary>
    public async Task<int> UpdateAllPostCommentsCountAsync()
    {
        var allPosts = await _postRepository.GetAllAsync();
        var updatedCount = 0;

        foreach (var post in allPosts)
        {
            // Comments collection'ı yüklenmişse direkt say
            if (post.Comments != null)
            {
                var count = post.Comments.Count;
                if (post.CommentsCount != count)
                {
                    post.CommentsCount = count;
                    await _postRepository.UpdateAsync(post);
                    updatedCount++;
                }
            }
        }

        return updatedCount;
    }

    /// <summary>
    /// Batch update: Tüm postları toplu olarak günceller (daha performanslı).
    /// </summary>
    public async Task<int> UpdateAllPostGeohashesBatchAsync()
    {
        var allPosts = await _postRepository.GetAllAsync();
        var postsToUpdate = new List<Post>();

        foreach (var post in allPosts)
        {
            double? latitude = null;
            double? longitude = null;

            // Önce post'taki direkt koordinatları kontrol et (yeni sistem)
            if (post.Latitude.HasValue && post.Longitude.HasValue)
            {
                latitude = post.Latitude.Value;
                longitude = post.Longitude.Value;
            }
            // Eğer post'ta koordinat yoksa ve PlaceId varsa, Place'den al (eski sistem)
            else if (post.PlaceId.HasValue && post.Place != null)
            {
                var place = post.Place;
                if (place == null || !place.Latitude.HasValue || !place.Longitude.HasValue)
                    continue;

                latitude = place.Latitude.Value;
                longitude = place.Longitude.Value;
            }
            else
            {
                // Ne post'ta ne de Place'de koordinat yok
                continue;
            }

            var needsUpdate = false;

            if (!post.Latitude.HasValue || !post.Longitude.HasValue)
            {
                needsUpdate = true;
            }
            else if (Math.Abs(post.Latitude.Value - latitude.Value) > 0.0001 ||
                     Math.Abs(post.Longitude.Value - longitude.Value) > 0.0001)
            {
                needsUpdate = true;
            }

            if (string.IsNullOrEmpty(post.Geohash))
            {
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                post.Latitude = latitude.Value;
                post.Longitude = longitude.Value;
                post.Geohash = GeohashUtil.Encode(latitude.Value, longitude.Value);
                postsToUpdate.Add(post);
            }
        }

        if (postsToUpdate.Any())
        {
            await _postRepository.UpdateRangeAsync(postsToUpdate);
        }

        return postsToUpdate.Count;
    }
}

