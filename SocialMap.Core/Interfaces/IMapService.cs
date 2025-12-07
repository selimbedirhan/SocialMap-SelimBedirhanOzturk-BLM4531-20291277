using SocialMap.Core.DTOs;

namespace SocialMap.Core.Interfaces;

public interface IMapService
{
    Task<IEnumerable<MapClusterDto>> GetClustersAsync(double north, double south, double east, double west, int zoom);
}

