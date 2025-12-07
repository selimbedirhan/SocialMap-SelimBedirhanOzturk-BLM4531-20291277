namespace SocialMap.Core.Entities;

public class Place
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Country { get; set; } = "Türkiye"; // Varsayılan
    public string? District { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Description { get; set; }
    public string? Tags { get; set; }  // "doğa,tarih,kültür"

    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

