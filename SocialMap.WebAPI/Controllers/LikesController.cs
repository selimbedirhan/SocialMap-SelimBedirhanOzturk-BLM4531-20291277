using Microsoft.AspNetCore.Mvc;
using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;

namespace SocialMap.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LikesController : ControllerBase
{
    private readonly ILikeService _likeService;

    public LikesController(ILikeService likeService)
    {
        _likeService = likeService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Like>> GetLikeById(Guid id)
    {
        var like = await _likeService.GetLikeByIdAsync(id);
        if (like == null)
            return NotFound();

        return Ok(like);
    }

    [HttpGet("post/{postId}")]
    public async Task<ActionResult<IEnumerable<Like>>> GetLikesByPost(Guid postId)
    {
        var likes = await _likeService.GetLikesByPostIdAsync(postId);
        return Ok(likes);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Like>>> GetLikesByUser(Guid userId)
    {
        var likes = await _likeService.GetLikesByUserIdAsync(userId);
        return Ok(likes);
    }

    [HttpGet("post/{postId}/user/{userId}/check")]
    public async Task<ActionResult<bool>> IsLiked(Guid postId, Guid userId)
    {
        var isLiked = await _likeService.IsPostLikedByUserAsync(postId, userId);
        return Ok(isLiked);
    }

    [HttpGet("post/{postId}/count")]
    public async Task<ActionResult<int>> GetLikeCount(Guid postId)
    {
        var count = await _likeService.GetLikeCountByPostIdAsync(postId);
        return Ok(count);
    }

    [HttpPost]
    public async Task<ActionResult<Like>> AddLike([FromQuery] Guid postId, [FromQuery] Guid userId)
    {
        try
        {
            var like = await _likeService.AddLikeAsync(postId, userId);
            return CreatedAtAction(nameof(GetLikeById), new { id = like.Id }, like);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete]
    public async Task<IActionResult> RemoveLike([FromQuery] Guid postId, [FromQuery] Guid userId)
    {
        try
        {
            await _likeService.RemoveLikeAsync(postId, userId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}

