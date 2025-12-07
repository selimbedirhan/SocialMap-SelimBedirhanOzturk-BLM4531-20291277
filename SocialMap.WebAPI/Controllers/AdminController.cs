using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMap.Business.Services;
using SocialMap.Repository.Data;

namespace SocialMap.WebAPI.Controllers;

/// <summary>
/// Admin/Utility işlemleri için controller.
/// Production'da authentication/authorization eklenmeli.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly PostGeohashUpdateService _geohashUpdateService;

    public AdminController(PostGeohashUpdateService geohashUpdateService)
    {
        _geohashUpdateService = geohashUpdateService;
    }

    /// <summary>
    /// Tüm postların geohash'lerini günceller.
    /// Place'den koordinatları alıp post'a kopyalar ve geohash hesaplar.
    /// </summary>
    [HttpPost("update-post-geohashes")]
    public async Task<ActionResult> UpdatePostGeohashes([FromQuery] bool useBatch = false)
    {
        try
        {
            int updatedCount;
            if (useBatch)
            {
                updatedCount = await _geohashUpdateService.UpdateAllPostGeohashesBatchAsync();
            }
            else
            {
                updatedCount = await _geohashUpdateService.UpdateAllPostGeohashesAsync();
            }
            return Ok(new { message = $"{updatedCount} post güncellendi.", updatedCount });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Tüm postların CommentsCount değerlerini günceller.
    /// </summary>
    [HttpPost("update-post-comments-count")]
    public async Task<ActionResult> UpdatePostCommentsCount()
    {
        try
        {
            var updatedCount = await _geohashUpdateService.UpdateAllPostCommentsCountAsync();
            return Ok(new { message = $"{updatedCount} post güncellendi.", updatedCount });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Database migration'ı manuel olarak çalıştırır (kolonları ekler).
    /// </summary>
    [HttpPost("run-migration")]
    public async Task<ActionResult> RunMigration([FromServices] ApplicationDbContext dbContext)
    {
        try
        {
            await DatabaseMigrationHelper.EnsurePostLocationColumnsExistAsync(dbContext);
            return Ok(new { message = "Migration başarıyla tamamlandı." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }
}

