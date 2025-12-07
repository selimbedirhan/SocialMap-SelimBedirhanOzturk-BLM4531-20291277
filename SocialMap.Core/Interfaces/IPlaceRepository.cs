using SocialMap.Core.Entities;

namespace SocialMap.Core.Interfaces;

public interface IPlaceRepository : IRepository<Place>
{
    Task<IEnumerable<Place>> GetByCityAsync(string city);
    Task<IEnumerable<Place>> SearchByNameAsync(string name);
    Task<IEnumerable<Place>> GetByCreatedByIdAsync(Guid createdById);

    /// <summary>
    /// Returns places that have coordinates within the given bounding box.
    /// </summary>
    Task<IEnumerable<Place>> GetWithinBoundsAsync(double minLatitude, double maxLatitude, double minLongitude, double maxLongitude);
}

