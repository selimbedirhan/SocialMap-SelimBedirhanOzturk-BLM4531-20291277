using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMap.Core.DTOs;
using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;
using AutoMapper;

namespace SocialMap.WebAPI.Controllers;

/// <summary>
/// Hashtag sistemi için API endpoint'leri
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HashtagsController : ControllerBase
{
    private readonly IHashtagRepository _hashtagRepository;
    private readonly IMapper _mapper;

    public HashtagsController(IHashtagRepository hashtagRepository, IMapper mapper)
    {
        _hashtagRepository = hashtagRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Trend hashtag'leri getir
    /// </summary>
    [HttpGet("trending")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<HashtagDto>>> GetTrending([FromQuery] int count = 10)
    {
        var hashtags = await _hashtagRepository.GetTrendingAsync(count);
        var dtos = hashtags.Select(h => new HashtagDto
        {
            Id = h.Id,
            Name = h.Name,
            UsageCount = h.UsageCount,
            LastUsedAt = h.LastUsedAt
        });
        return Ok(dtos);
    }

    /// <summary>
    /// Hashtag ara
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<HashtagDto>>> Search([FromQuery] string q, [FromQuery] int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Arama terimi gerekli");

        var hashtags = await _hashtagRepository.SearchAsync(q, limit);
        var dtos = hashtags.Select(h => new HashtagDto
        {
            Id = h.Id,
            Name = h.Name,
            UsageCount = h.UsageCount,
            LastUsedAt = h.LastUsedAt
        });
        return Ok(dtos);
    }

    /// <summary>
    /// Belirli hashtag'e sahip postları getir
    /// </summary>
    [HttpGet("{name}/posts")]
    [AllowAnonymous]
    public async Task<ActionResult<PaginatedResponse<PostResponseDto>>> GetPostsByHashtag(
        string name,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var posts = await _hashtagRepository.GetPostsByHashtagAsync(name, page, pageSize);
        var postDtos = _mapper.Map<IEnumerable<PostResponseDto>>(posts);

        return Ok(new PaginatedResponse<PostResponseDto>
        {
            Items = postDtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = posts.Count()
        });
    }
}

public class HashtagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public DateTime LastUsedAt { get; set; }
}
