using AutoMapper;
using SocialMap.Core.DTOs;
using SocialMap.Core.Entities;

namespace SocialMap.Business.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User Mapping
        CreateMap<User, UserResponseDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username ?? ""));

        // Place Mapping
        CreateMap<Place, PlaceResponseDto>()
            .ForMember(dest => dest.CreatedByUsername, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.Username : ""));

        // Comment Mapping
        CreateMap<Comment, CommentResponseDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User != null ? src.User.Username : ""))
            .ForMember(dest => dest.UserProfilePhotoUrl, opt => opt.MapFrom(src => src.User != null ? src.User.ProfilePhotoUrl : null));

        // Like Mapping
        CreateMap<Like, LikeResponseDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User != null ? src.User.Username : ""))
            .ForMember(dest => dest.UserProfilePhotoUrl, opt => opt.MapFrom(src => src.User != null ? src.User.ProfilePhotoUrl : null));

        // Post Mapping
        CreateMap<Post, PostResponseDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User != null ? src.User.Username : ""))
            .ForMember(dest => dest.UserProfilePhotoUrl, opt => opt.MapFrom(src => src.User != null ? src.User.ProfilePhotoUrl : null))
            .ForMember(dest => dest.PlaceName, opt => opt.MapFrom(src => src.PlaceName ?? (src.Place != null ? src.Place.Name : "")))
            .ForMember(dest => dest.PlaceLocation, opt => opt.MapFrom((src, dest) => 
            {
                string placeName = src.PlaceName ?? src.Place?.Name ?? "";
                string city = src.City ?? src.Place?.City ?? "";
                string country = src.Country ?? src.Place?.Country ?? "Türkiye";
                
                return !string.IsNullOrWhiteSpace(placeName)
                    ? $"{placeName}" + (!string.IsNullOrWhiteSpace(city) ? $" - {city}" : "") + (!string.IsNullOrWhiteSpace(country) ? $" - {country}" : "")
                    : "";
            }))
            .ForMember(dest => dest.CommentsCount, opt => opt.MapFrom(src => 
                src.CommentsCount == 0 && src.Comments != null && src.Comments.Any() 
                ? src.Comments.Count 
                : src.CommentsCount));
    }
}
