namespace SocialMap.Core.DTOs;

public class PlaceResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string? District { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Description { get; set; }
    public string? Tags { get; set; }
    public Guid CreatedById { get; set; }
    public string CreatedByUsername { get; set; } = null!;
    public int PostsCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

