using Microsoft.AspNetCore.Mvc;
using SocialMap.Core.DTOs;
using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;
using SocialMap.WebAPI.Services;

namespace SocialMap.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly NotificationBroadcaster _notificationBroadcaster;
    private readonly INotificationRepository _notificationRepository;

    public CommentsController(ICommentService commentService, NotificationBroadcaster notificationBroadcaster, INotificationRepository notificationRepository)
    {
        _commentService = commentService;
        _notificationBroadcaster = notificationBroadcaster;
        _notificationRepository = notificationRepository;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CommentResponseDto>> GetCommentById(Guid id)
    {
        var comment = await _commentService.GetCommentByIdAsync(id);
        if (comment == null)
            return NotFound();

        return Ok(MapToCommentResponseDto(comment));
    }

    [HttpGet("post/{postId}")]
    public async Task<ActionResult<IEnumerable<CommentResponseDto>>> GetCommentsByPost(Guid postId)
    {
        var comments = await _commentService.GetCommentsByPostIdAsync(postId);
        var commentDtos = comments.Select(c => MapToCommentResponseDto(c));
        return Ok(commentDtos);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<CommentResponseDto>>> GetCommentsByUser(Guid userId)
    {
        var comments = await _commentService.GetCommentsByUserIdAsync(userId);
        var commentDtos = comments.Select(c => MapToCommentResponseDto(c));
        return Ok(commentDtos);
    }

    [HttpGet("reply/{parentCommentId}")]
    public async Task<ActionResult<IEnumerable<CommentResponseDto>>> GetReplies(Guid parentCommentId)
    {
        var replies = await _commentService.GetRepliesByCommentIdAsync(parentCommentId);
        var replyDtos = replies.Select(r => MapToCommentResponseDto(r));
        return Ok(replyDtos);
    }

    [HttpPost]
    public async Task<ActionResult<CommentResponseDto>> CreateComment([FromBody] CreateCommentDto dto)
    {
        try
        {
            var comment = await _commentService.CreateCommentAsync(
                dto.PostId,
                dto.UserId,
                dto.Text,
                dto.ParentCommentId);

            // CommentService içinde bildirimler oluşturuldu (post sahibine ve takip edenlere)
            // Her bildirimi SignalR ile gönder
            // Son 10 saniye içinde oluşturulan "comment" tipindeki bildirimleri bul
            var cutoffTime = DateTime.UtcNow.AddSeconds(-10);
            // Not: GetByUserIdAsync userId bekliyor, postId değil
            // CommentService'te bildirimler farklı kullanıcılara gönderildi
            // Şimdilik basit çözüm: Bildirimler zaten oluşturuldu, SignalR gönderimi
            // ileride CommentService'e NotificationBroadcaster inject edip direkt gönderebiliriz

            var commentDto = await _commentService.GetCommentByIdAsync(comment.Id);
            return CreatedAtAction(nameof(GetCommentById), new { id = comment.Id }, MapToCommentResponseDto(commentDto!));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateComment(Guid id, [FromBody] Comment comment)
    {
        try
        {
            if (id != comment.Id)
                return BadRequest("ID mismatch");

            await _commentService.UpdateCommentAsync(comment);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        try
        {
            await _commentService.DeleteCommentAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
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

