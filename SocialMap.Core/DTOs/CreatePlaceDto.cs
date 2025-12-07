namespace SocialMap.Core.DTOs;

public class CreatePlaceDto
{
    public string Name { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Country { get; set; } = "Türkiye";
    public string? District { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Description { get; set; }
    public string? Tags { get; set; }
    public Guid CreatedById { get; set; }
}

