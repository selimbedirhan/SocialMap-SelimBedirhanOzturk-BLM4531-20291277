using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SocialMap.Core.DTOs;
using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;

namespace SocialMap.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlacesController : ControllerBase
{
    private readonly IPlaceService _placeService;
    private readonly IMapper _mapper;

    public PlacesController(IPlaceService placeService, IMapper mapper)
    {
        _placeService = placeService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlaceResponseDto>>> GetAllPlaces()
    {
        var places = await _placeService.GetAllPlacesAsync();
        var placeDtos = _mapper.Map<IEnumerable<PlaceResponseDto>>(places);
        return Ok(placeDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PlaceResponseDto>> GetPlaceById(Guid id)
    {
        var place = await _placeService.GetPlaceByIdAsync(id);
        if (place == null)
            return NotFound();

        return Ok(_mapper.Map<PlaceResponseDto>(place));
    }

    [HttpGet("city/{city}")]
    public async Task<ActionResult<IEnumerable<PlaceResponseDto>>> GetPlacesByCity(string city)
    {
        var places = await _placeService.GetPlacesByCityAsync(city);
        var placeDtos = _mapper.Map<IEnumerable<PlaceResponseDto>>(places);
        return Ok(placeDtos);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<PlaceResponseDto>>> SearchPlaces([FromQuery] string term)
    {
        var places = await _placeService.SearchPlacesAsync(term);
        var placeDtos = _mapper.Map<IEnumerable<PlaceResponseDto>>(places);
        return Ok(placeDtos);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<PlaceResponseDto>>> GetPlacesByUser(Guid userId)
    {
        var places = await _placeService.GetPlacesByCreatedUserIdAsync(userId);
        var placeDtos = _mapper.Map<IEnumerable<PlaceResponseDto>>(places);
        return Ok(placeDtos);
    }

    [HttpPost]
    public async Task<ActionResult<PlaceResponseDto>> CreatePlace([FromBody] CreatePlaceDto dto)
    {
        try
        {
            var place = await _placeService.CreatePlaceAsync(
                dto.Name,
                dto.City,
                dto.Country,
                dto.District,
                dto.Latitude,
                dto.Longitude,
                dto.Description,
                dto.Tags,
                dto.CreatedById);

            var placeDto = await _placeService.GetPlaceByIdAsync(place.Id);
            return CreatedAtAction(nameof(GetPlaceById), new { id = place.Id }, _mapper.Map<PlaceResponseDto>(placeDto!));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePlace(Guid id, [FromBody] Place place)
    {
        try
        {
            if (id != place.Id)
                return BadRequest("ID mismatch");

            await _placeService.UpdatePlaceAsync(place);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlace(Guid id)
    {
        try
        {
            await _placeService.DeletePlaceAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }


}

