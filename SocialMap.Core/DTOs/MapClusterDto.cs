namespace SocialMap.Core.DTOs;

public class MapClusterDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int PostsCount { get; set; }
    public bool IsCluster { get; set; } = true; // true = cluster, false = single post/place
    public List<Guid>? SamplePostIds { get; set; } // Cluster içindeki örnek post ID'leri

    public Guid? PlaceId { get; set; }
    public string? PlaceName { get; set; }
    public string? City { get; set; }
}


