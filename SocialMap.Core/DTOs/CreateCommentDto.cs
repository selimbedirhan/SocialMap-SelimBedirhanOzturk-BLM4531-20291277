namespace SocialMap.Core.DTOs;

public class CreateCommentDto
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string Text { get; set; } = null!;
    public Guid? ParentCommentId { get; set; }
}

