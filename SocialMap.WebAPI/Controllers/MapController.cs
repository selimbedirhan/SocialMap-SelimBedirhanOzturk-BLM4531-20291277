using Microsoft.AspNetCore.Mvc;
using SocialMap.Core.DTOs;
using SocialMap.Core.Interfaces;

namespace SocialMap.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MapController : ControllerBase
{
    private readonly IMapService _mapService;

    public MapController(IMapService mapService)
    {
        _mapService = mapService;
    }

    /// <summary>
    /// Returns clusters based on posts within the given bounding box and zoom level.
    /// Uses geohash-based clustering for low zoom levels and tile-based clustering for high zoom levels.
    /// </summary>
    [HttpGet("clusters")]
    public async Task<ActionResult<IEnumerable<MapClusterDto>>> GetClusters(
        [FromQuery] double? north = null,
        [FromQuery] double? south = null,
        [FromQuery] double? east = null,
        [FromQuery] double? west = null,
        [FromQuery] int zoom = 10)
    {
        // Dünya geneli için varsayılan bbox
        var minLat = south ?? -90;
        var maxLat = north ?? 90;
        var minLng = west ?? -180;
        var maxLng = east ?? 180;

        if (minLat > maxLat || minLng > maxLng)
        {
            return BadRequest("Geçersiz koordinat aralığı.");
        }

        // Zoom seviyesini sınırla (0-18 arası)
        zoom = Math.Max(0, Math.Min(18, zoom));

        var clusters = await _mapService.GetClustersAsync(maxLat, minLat, maxLng, minLng, zoom);

        return Ok(clusters);
    }
}


