using SocialMap.Core.Entities;

namespace SocialMap.Core.Interfaces;

public interface IPlaceService
{
    Task<Place?> GetPlaceByIdAsync(Guid id);
    Task<IEnumerable<Place>> GetAllPlacesAsync();
    Task<IEnumerable<Place>> GetPlacesByCityAsync(string city);
    Task<IEnumerable<Place>> SearchPlacesAsync(string searchTerm);
    Task<IEnumerable<Place>> GetPlacesByCreatedUserIdAsync(Guid userId);
    Task<IEnumerable<Place>> GetPlacesWithinBoundsAsync(double minLatitude, double maxLatitude, double minLongitude, double maxLongitude);
    Task<Place> CreatePlaceAsync(string name, string city, string country, string? district, double? latitude, double? longitude, string? description, string? tags, Guid createdById);
    Task UpdatePlaceAsync(Place place);
    Task DeletePlaceAsync(Guid id);
}

