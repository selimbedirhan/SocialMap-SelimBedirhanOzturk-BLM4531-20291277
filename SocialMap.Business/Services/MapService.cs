using SocialMap.Core.DTOs;
using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;
using SocialMap.Business.Utils;

namespace SocialMap.Business.Services;

public class MapService : IMapService
{
    private readonly IPostRepository _postRepository;
    private const int MaxClusters = 500; // Performans için maksimum cluster sayısı
    private const int MaxPostsPerCluster = 10; // Her cluster'da gösterilecek örnek post sayısı

    public MapService(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<IEnumerable<MapClusterDto>> GetClustersAsync(double north, double south, double east, double west, int zoom)
    {
        // Bounding box doğrulama
        if (north <= south || east <= west)
            return Enumerable.Empty<MapClusterDto>();

        // Zoom seviyesine göre geohash prefix uzunluğunu belirle
        var geohashPrefixLength = GeohashUtil.GetPrefixLengthForZoom(zoom);

        // Bounding box içindeki tüm postları al
        var posts = await _postRepository.GetPostsWithinBoundsAsync(south, north, west, east);

        // Koordinatları olan postları filtrele
        var validPosts = posts
            .Where(p => p.Latitude.HasValue && p.Longitude.HasValue)
            .ToList();

        if (!validPosts.Any())
            return Enumerable.Empty<MapClusterDto>();

        // Zoom seviyesine göre clustering stratejisi
        if (zoom <= 9)
        {
            // Düşük zoom: Geohash prefix bazlı clustering
            return ClusterByGeohash(validPosts, geohashPrefixLength);
        }
        else
        {
            // Yüksek zoom: Tile-based clustering (daha hassas)
            return ClusterByTiles(validPosts, north, south, east, west, zoom);
        }
    }

    /// <summary>
    /// Geohash prefix bazlı clustering - düşük zoom seviyeleri için
    /// </summary>
    private IEnumerable<MapClusterDto> ClusterByGeohash(List<Post> posts, int prefixLength)
    {
        var clusterDict = new Dictionary<string, List<Post>>();

        foreach (var post in posts)
        {
            if (string.IsNullOrEmpty(post.Geohash) || post.Geohash.Length < prefixLength)
            {
                // Geohash yoksa veya yetersizse, koordinatlardan hesapla
                if (post.Latitude.HasValue && post.Longitude.HasValue)
                {
                    var geohash = GeohashUtil.Encode(post.Latitude.Value, post.Longitude.Value, prefixLength);
                    var prefix = geohash.Substring(0, Math.Min(prefixLength, geohash.Length));
                    
                    if (!clusterDict.ContainsKey(prefix))
                        clusterDict[prefix] = new List<Post>();
                    clusterDict[prefix].Add(post);
                }
            }
            else
            {
                var prefix = post.Geohash.Substring(0, Math.Min(prefixLength, post.Geohash.Length));
                if (!clusterDict.ContainsKey(prefix))
                    clusterDict[prefix] = new List<Post>();
                clusterDict[prefix].Add(post);
            }
        }

        var clusters = new List<MapClusterDto>();

        foreach (var kvp in clusterDict)
        {
            var clusterPosts = kvp.Value;
            if (clusterPosts.Count == 0) continue;

            // Cluster merkezini hesapla (ortalama koordinat)
            var avgLat = clusterPosts.Average(p => p.Latitude!.Value);
            var avgLng = clusterPosts.Average(p => p.Longitude!.Value);

            // Örnek postları seç (en yeni veya en çok beğenilen)
            var samplePosts = clusterPosts
                .OrderByDescending(p => p.CreatedAt)
                .Take(MaxPostsPerCluster)
                .Select(p => p.Id)
                .ToList();

            clusters.Add(new MapClusterDto
            {
                Latitude = avgLat,
                Longitude = avgLng,
                PostsCount = clusterPosts.Count,
                SamplePostIds = samplePosts,
                IsCluster = clusterPosts.Count > 1
            });
        }

        // Maksimum cluster sayısını aşmamak için en yoğun cluster'ları seç
        return clusters
            .OrderByDescending(c => c.PostsCount)
            .Take(MaxClusters)
            .ToList();
    }

    /// <summary>
    /// Tile-based clustering - yüksek zoom seviyeleri için
    /// </summary>
    private IEnumerable<MapClusterDto> ClusterByTiles(List<Post> posts, double north, double south, double east, double west, int zoom)
    {
        // Zoom seviyesine göre hücre boyutunu hesapla
        // Yüksek zoom = küçük hücreler, düşük zoom = büyük hücreler
        var cellSize = CalculateCellSize(zoom, north - south, east - west);

        var clusterDict = new Dictionary<string, List<Post>>();

        foreach (var post in posts)
        {
            if (!post.Latitude.HasValue || !post.Longitude.HasValue)
                continue;

            var lat = post.Latitude.Value;
            var lng = post.Longitude.Value;

            // Hangi hücreye düştüğünü hesapla
            var cellX = (int)Math.Floor((lng - west) / cellSize.Longitude);
            var cellY = (int)Math.Floor((lat - south) / cellSize.Latitude);
            var cellKey = $"{cellX}:{cellY}";

            if (!clusterDict.ContainsKey(cellKey))
                clusterDict[cellKey] = new List<Post>();
            clusterDict[cellKey].Add(post);
        }

        var clusters = new List<MapClusterDto>();

        foreach (var kvp in clusterDict)
        {
            var clusterPosts = kvp.Value;
            if (clusterPosts.Count == 0) continue;

            // Cluster merkezini hesapla
            var avgLat = clusterPosts.Average(p => p.Latitude!.Value);
            var avgLng = clusterPosts.Average(p => p.Longitude!.Value);

            // Örnek postları seç
            var samplePosts = clusterPosts
                .OrderByDescending(p => p.CreatedAt)
                .Take(MaxPostsPerCluster)
                .Select(p => p.Id)
                .ToList();

            // Tek post ise Place bilgilerini ekle
            var firstPost = clusterPosts.First();
            var placeId = firstPost.PlaceId;
            // Yeni sistem: Post'ta direkt yer bilgileri varsa onları kullan, yoksa Place'den al
            var placeName = firstPost.PlaceName ?? firstPost.Place?.Name;
            var city = firstPost.City ?? firstPost.Place?.City;

            clusters.Add(new MapClusterDto
            {
                Latitude = avgLat,
                Longitude = avgLng,
                PostsCount = clusterPosts.Count,
                SamplePostIds = samplePosts,
                PlaceId = clusterPosts.Count == 1 ? placeId : null,
                PlaceName = clusterPosts.Count == 1 ? placeName : null,
                City = clusterPosts.Count == 1 ? city : null,
                IsCluster = clusterPosts.Count > 1
            });
        }

        return clusters
            .OrderByDescending(c => c.PostsCount)
            .Take(MaxClusters)
            .ToList();
    }

    /// <summary>
    /// Zoom seviyesine göre hücre boyutunu hesapla
    /// </summary>
    private (double Latitude, double Longitude) CalculateCellSize(int zoom, double latRange, double lngRange)
    {
        // Zoom seviyesine göre hücre sayısı (2^zoom mantığı)
        var cellCount = Math.Pow(2, Math.Max(0, zoom - 6));
        
        // Minimum ve maksimum hücre boyutları
        var minCellSize = 0.001; // ~100m
        var maxCellSize = 1.0;   // ~100km

        var latCellSize = Math.Max(minCellSize, Math.Min(maxCellSize, latRange / cellCount));
        var lngCellSize = Math.Max(minCellSize, Math.Min(maxCellSize, lngRange / cellCount));

        return (latCellSize, lngCellSize);
    }
}

