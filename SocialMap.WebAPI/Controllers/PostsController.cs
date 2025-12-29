using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SocialMap.Core.DTOs;
using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;
using SocialMap.WebAPI.Services;

namespace SocialMap.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly NotificationBroadcaster _notificationBroadcaster;
    private readonly INotificationRepository _notificationRepository;
    private readonly IMapper _mapper;

    public PostsController(IPostService postService, NotificationBroadcaster notificationBroadcaster, INotificationRepository notificationRepository, IMapper mapper)
    {
        _postService = postService;
        _notificationBroadcaster = notificationBroadcaster;
        _notificationRepository = notificationRepository;
        _mapper = mapper;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetRecentPosts([FromQuery] int count = 20)
    {
        try
        {
            var posts = await _postService.GetRecentPostsAsync(count);
            var postDtos = _mapper.Map<IEnumerable<PostResponseDto>>(posts);
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

    /// <summary>
    /// Sayfalanmış gönderi listesi
    /// </summary>
    [HttpGet("paged")]
    [AllowAnonymous]
    public async Task<ActionResult<SocialMap.Core.DTOs.PaginatedResponse<PostResponseDto>>> GetRecentPostsPaged(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 1;
            if (pageSize > 100) pageSize = 100;

            var (items, totalCount) = await _postService.GetRecentPostsPagedAsync(page, pageSize);
            var postDtos = _mapper.Map<IEnumerable<PostResponseDto>>(items);

            var response = new SocialMap.Core.DTOs.PaginatedResponse<PostResponseDto>
            {
                Items = postDtos,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            #if DEBUG
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            #else
            return StatusCode(500, new { error = ex.Message });
            #endif
        }
    }


    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<PostResponseDto>> GetPostById(Guid id)
    {
        var post = await _postService.GetPostByIdAsync(id);
        if (post == null)
            return NotFound();

        return Ok(_mapper.Map<PostResponseDto>(post));
    }

    [HttpGet("user/{userId}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetPostsByUser(Guid userId)
    {
        try
        {
            var posts = await _postService.GetPostsByUserIdAsync(userId);
            var postDtos = _mapper.Map<IEnumerable<PostResponseDto>>(posts);
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
            var postDtos = _mapper.Map<IEnumerable<PostResponseDto>>(posts);
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
            return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, _mapper.Map<PostResponseDto>(postDto!));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePost(Guid id, [FromBody] UpdatePostDto dto)
    {
        try
        {
            var existingPost = await _postService.GetPostByIdAsync(id);
            if (existingPost == null)
                return NotFound("Post not found");

            existingPost.Caption = dto.Caption;
            // Optionally update other fields if needed in future

            await _postService.UpdatePostAsync(existingPost);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message); // Or BadRequest depending on logic
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
}

