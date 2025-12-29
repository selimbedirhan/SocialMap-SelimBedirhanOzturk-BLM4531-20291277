namespace SocialMap.Core.DTOs;

public class CommentResponseDto
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = null!;
    public string? UserProfilePhotoUrl { get; set; }
    public string Text { get; set; } = null!;
    public Guid? ParentCommentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CommentResponseDto> Replies { get; set; } = new();
}

