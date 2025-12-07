namespace SocialMap.Core.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Type { get; set; } = null!; // "like", "comment", "follow", "mention"
    public string Message { get; set; } = null!;
    public Guid? RelatedPostId { get; set; }
    public Post? RelatedPost { get; set; }
    public Guid? RelatedUserId { get; set; }
    public User? RelatedUser { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

