using Microsoft.AspNetCore.Mvc;
using SocialMap.Core.DTOs;
using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;
using SocialMap.WebAPI.Services;

namespace SocialMap.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly NotificationBroadcaster _notificationBroadcaster;
    private readonly INotificationRepository _notificationRepository;

    public PostsController(IPostService postService, NotificationBroadcaster notificationBroadcaster, INotificationRepository notificationRepository)
    {
        _postService = postService;
        _notificationBroadcaster = notificationBroadcaster;
        _notificationRepository = notificationRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetRecentPosts([FromQuery] int count = 20)
    {
        try
        {
            var posts = await _postService.GetRecentPostsAsync(count);
            var postDtos = posts.Select(p => MapToPostResponseDto(p));
            return Ok(postDtos);
        }
        catch (Exception ex)
        {
            // Development'ta detaylı hata göster
            #if DEBUG
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
            #else
            return StatusCode(500, new { error = ex.Message });
            #endif
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PostResponseDto>> GetPostById(Guid id)
    {
        var post = await _postService.GetPostByIdAsync(id);
        if (post == null)
            return NotFound();

        return Ok(MapToPostResponseDto(post));
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetPostsByUser(Guid userId)
    {
        try
        {
            var posts = await _postService.GetPostsByUserIdAsync(userId);
            var postDtos = posts.Select(p => MapToPostResponseDto(p));
            return Ok(postDtos);
        }
        catch (Exception ex)
        {
            // Development'ta detaylı hata göster
            #if DEBUG
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
            #else
            return StatusCode(500, new { error = ex.Message });
            #endif
        }
    }

    [HttpGet("place/{placeId}")]
    public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetPostsByPlace(Guid placeId)
    {
        try
        {
            var posts = await _postService.GetPostsByPlaceIdAsync(placeId);
            var postDtos = posts.Select(p => MapToPostResponseDto(p));
            return Ok(postDtos);
        }
        catch (Exception ex)
        {
            // Development'ta detaylı hata göster
            #if DEBUG
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace, innerException = ex.InnerException?.Message });
            #else
            return StatusCode(500, new { error = ex.Message });
            #endif
        }
    }

    [HttpPost]
    public async Task<ActionResult<PostResponseDto>> CreatePost([FromBody] CreatePostDto dto)
    {
        try
        {
            var post = await _postService.CreatePostAsync(
                dto.UserId,
                dto.PlaceId,
                dto.PlaceName,
                dto.Latitude,
                dto.Longitude,
                dto.City,
                dto.Country,
                dto.MediaUrl,
                dto.Caption);

            // PostService içinde takip edenlere bildirimler oluşturuldu
            // Her takip eden için ayrı bildirim var, hepsini SignalR ile gönder
            // Son 10 saniye içinde oluşturulan "post" tipindeki bildirimleri bul ve gönder
            var cutoffTime = DateTime.UtcNow.AddSeconds(-10);
            var allNotifications = await _notificationRepository.GetByUserIdAsync(dto.UserId);
            var recentPostNotifications = allNotifications
                .Where(n => n.Type == "post" && n.RelatedPostId == post.Id && n.CreatedAt >= cutoffTime)
                .ToList();
            
            // Ama bu sadece post sahibinin bildirimlerini getiriyor, takip edenlerin bildirimlerini değil
            // Daha iyi çözüm: PostService'te bildirim oluşturulduktan sonra direkt SignalR göndermek
            // Şimdilik bu şekilde bırakalım - PostService'te bildirimler oluşturuldu ama SignalR gönderilmedi
            // İleride PostService'e NotificationBroadcaster inject edip direkt gönderebiliriz

            var postDto = await _postService.GetPostByIdAsync(post.Id);
            return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, MapToPostResponseDto(postDto!));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePost(Guid id, [FromBody] Post post)
    {
        try
        {
            if (id != post.Id)
                return BadRequest("ID mismatch");

            await _postService.UpdatePostAsync(post);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        try
        {
            await _postService.DeletePostAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    private static PostResponseDto MapToPostResponseDto(Post post)
    {
        if (post == null)
            throw new ArgumentNullException(nameof(post));

        // Yer bilgilerini belirle - önce direkt post'tan, yoksa Place'den
        string placeName = post.PlaceName ?? post.Place?.Name ?? "";
        string city = post.City ?? post.Place?.City ?? "";
        string country = post.Country ?? post.Place?.Country ?? "Türkiye";
        
        var placeLocation = !string.IsNullOrWhiteSpace(placeName)
            ? $"{placeName}" + (!string.IsNullOrWhiteSpace(city) ? $" - {city}" : "") + (!string.IsNullOrWhiteSpace(country) ? $" - {country}" : "")
            : "";

        // CommentsCount için fallback: Eğer property 0 ise ve Comments collection varsa, onu kullan
        var commentsCount = post.CommentsCount;
        if (commentsCount == 0 && post.Comments != null && post.Comments.Any())
        {
            commentsCount = post.Comments.Count;
        }

        return new PostResponseDto
        {
            Id = post.Id,
            UserId = post.UserId,
            Username = post.User?.Username ?? "",
            PlaceId = post.PlaceId,
            PlaceName = placeName,
            PlaceLocation = placeLocation,
            Latitude = post.Latitude,
            Longitude = post.Longitude,
            MediaUrl = post.MediaUrl,
            Caption = post.Caption,
            LikesCount = post.LikesCount,
            CommentsCount = commentsCount,
            CreatedAt = post.CreatedAt,
            Comments = post.Comments?.Select(c => MapToCommentResponseDto(c)).ToList() ?? new List<CommentResponseDto>(),
            Likes = post.Likes?.Select(l => new LikeResponseDto
            {
                Id = l.Id,
                PostId = l.PostId,
                UserId = l.UserId,
                Username = l.User?.Username ?? "",
                CreatedAt = l.CreatedAt
            }).ToList() ?? new List<LikeResponseDto>()
        };
    }

    private static CommentResponseDto MapToCommentResponseDto(Comment comment)
    {
        return new CommentResponseDto
        {
            Id = comment.Id,
            PostId = comment.PostId,
            UserId = comment.UserId,
            Username = comment.User?.Username ?? "",
            Text = comment.Text,
            ParentCommentId = comment.ParentCommentId,
            CreatedAt = comment.CreatedAt,
            Replies = comment.Replies?.Select(r => MapToCommentResponseDto(r)).ToList() ?? new List<CommentResponseDto>()
        };
    }
}

