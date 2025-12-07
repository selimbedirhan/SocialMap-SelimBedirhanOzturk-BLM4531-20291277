using Microsoft.AspNetCore.Mvc;
using SocialMap.Core.DTOs;
using SocialMap.Core.Interfaces;

namespace SocialMap.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IPlaceService _placeService;
    private readonly IPostService _postService;

    public SearchController(IUserService userService, IPlaceService placeService, IPostService postService)
    {
        _userService = userService;
        _placeService = placeService;
        _postService = postService;
    }

    [HttpGet("all")]
    public async Task<ActionResult<object>> SearchAll([FromQuery] string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return BadRequest("Arama terimi gerekli.");

        var users = await _userService.SearchUsersAsync(term);
        var places = await _placeService.SearchPlacesAsync(term);
        var posts = await _postService.GetRecentPostsAsync(100); // Tüm gönderileri al, sonra filtrele

        var filteredPosts = posts.Where(p => 
            (p.Caption != null && p.Caption.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
            (p.Place?.Name != null && p.Place.Name.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
            (p.Place?.City != null && p.Place.City.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
            (p.Place?.Country != null && p.Place.Country.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
            (p.Place?.District != null && p.Place.District.Contains(term, StringComparison.OrdinalIgnoreCase))
        );

        return Ok(new
        {
            users = users.Select(u => new UserResponseDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                ProfilePhotoUrl = u.ProfilePhotoUrl,
                Bio = u.Bio,
                CreatedAt = u.CreatedAt
            }),
            places = places.Select(p => new PlaceResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                City = p.City,
                District = p.District,
                Description = p.Description,
                Tags = p.Tags,
                CreatedAt = p.CreatedAt
            }),
            posts = filteredPosts.Select(p => new
            {
                p.Id,
                p.Caption,
                p.MediaUrl,
                p.LikesCount,
                commentsCount = p.CommentsCount,
                p.CreatedAt,
                username = p.User?.Username,
                placeName = p.Place?.Name
            })
        });
    }

    [HttpGet("posts")]
    public async Task<ActionResult<IEnumerable<object>>> SearchPosts(
        [FromQuery] string? term = null,
        [FromQuery] string? city = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? sortBy = "newest")
    {
        var posts = await _postService.GetRecentPostsAsync(1000);

        if (!string.IsNullOrWhiteSpace(term))
        {
            posts = posts.Where(p =>
                (p.Caption != null && p.Caption.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                (p.Place?.Name != null && p.Place.Name.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                (p.Place?.City != null && p.Place.City.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                (p.Place?.Country != null && p.Place.Country.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                (p.Place?.District != null && p.Place.District.Contains(term, StringComparison.OrdinalIgnoreCase))
            );
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            posts = posts.Where(p => p.Place?.City != null && 
                p.Place.City.Contains(city, StringComparison.OrdinalIgnoreCase));
        }

        if (fromDate.HasValue)
        {
            posts = posts.Where(p => p.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            posts = posts.Where(p => p.CreatedAt <= toDate.Value);
        }

        // Sıralama
        var postsList = posts.ToList();
        postsList = sortBy?.ToLower() switch
        {
            "likes" => postsList.OrderByDescending(p => p.LikesCount).ToList(),
            "comments" => postsList.OrderByDescending(p => p.CommentsCount).ToList(),
            "oldest" => postsList.OrderBy(p => p.CreatedAt).ToList(),
            _ => postsList.OrderByDescending(p => p.CreatedAt).ToList()
        };

        return Ok(postsList.Select(p => new
        {
            p.Id,
            p.Caption,
            p.MediaUrl,
            p.LikesCount,
            commentsCount = p.Comments?.Count ?? 0,
            p.CreatedAt,
            username = p.User?.Username,
            placeName = p.Place?.Name,
            city = p.Place?.City
        }));
    }
}

