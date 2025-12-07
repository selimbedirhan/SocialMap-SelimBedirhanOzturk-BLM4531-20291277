using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;

namespace SocialMap.Business.Services;

public class PlaceService : IPlaceService
{
    private readonly IPlaceRepository _placeRepository;
    private readonly IUserRepository _userRepository;

    public PlaceService(IPlaceRepository placeRepository, IUserRepository userRepository)
    {
        _placeRepository = placeRepository;
        _userRepository = userRepository;
    }

    public async Task<Place?> GetPlaceByIdAsync(Guid id)
    {
        return await _placeRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Place>> GetAllPlacesAsync()
    {
        return await _placeRepository.GetAllAsync();
    }

    public async Task<IEnumerable<Place>> GetPlacesByCityAsync(string city)
    {
        return await _placeRepository.GetByCityAsync(city);
    }

    public async Task<IEnumerable<Place>> SearchPlacesAsync(string searchTerm)
    {
        return await _placeRepository.SearchByNameAsync(searchTerm);
    }

    public async Task<IEnumerable<Place>> GetPlacesByCreatedUserIdAsync(Guid userId)
    {
        return await _placeRepository.GetByCreatedByIdAsync(userId);
    }

    public async Task<IEnumerable<Place>> GetPlacesWithinBoundsAsync(double minLatitude, double maxLatitude, double minLongitude, double maxLongitude)
    {
        return await _placeRepository.GetWithinBoundsAsync(minLatitude, maxLatitude, minLongitude, maxLongitude);
    }

    public async Task<Place> CreatePlaceAsync(string name, string city, string country, string? district, double? latitude, double? longitude, string? description, string? tags, Guid createdById)
    {
        var user = await _userRepository.GetByIdAsync(createdById);
        if (user == null)
            throw new InvalidOperationException($"User with ID '{createdById}' not found.");

        var place = new Place
        {
            Id = Guid.NewGuid(),
            Name = name,
            City = city,
            Country = country,
            District = district,
            Latitude = latitude,
            Longitude = longitude,
            Description = description,
            Tags = tags,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow
        };

        return await _placeRepository.AddAsync(place);
    }

    public async Task UpdatePlaceAsync(Place place)
    {
        var existingPlace = await _placeRepository.GetByIdAsync(place.Id);
        if (existingPlace == null)
            throw new InvalidOperationException($"Place with ID '{place.Id}' not found.");

        await _placeRepository.UpdateAsync(place);
    }

    public async Task DeletePlaceAsync(Guid id)
    {
        var place = await _placeRepository.GetByIdAsync(id);
        if (place == null)
            throw new InvalidOperationException($"Place with ID '{id}' not found.");

        await _placeRepository.DeleteAsync(place);
    }
}

