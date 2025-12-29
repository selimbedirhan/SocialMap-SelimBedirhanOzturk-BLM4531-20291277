using FluentValidation;
using SocialMap.Core.DTOs;

namespace SocialMap.Business.Validators;

public class CreatePostDtoValidator : AbstractValidator<CreatePostDto>
{
    public CreatePostDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID gereklidir.");

        // Yer validation: Ya PlaceId olmalı ya da PlaceName + Koordinatlar
        RuleFor(x => x)
            .Must(x => x.PlaceId.HasValue || (!string.IsNullOrEmpty(x.PlaceName) && x.Latitude.HasValue && x.Longitude.HasValue))
            .WithMessage("Bir yer seçmelisiniz veya yer adı ve konum bilgisi girmelisiniz.");

        RuleFor(x => x.Latitude)
            .NotNull().When(x => !x.PlaceId.HasValue)
            .WithMessage("Enlem bilgisi gereklidir.");

        RuleFor(x => x.Longitude)
            .NotNull().When(x => !x.PlaceId.HasValue)
            .WithMessage("Boylam bilgisi gereklidir.");

        RuleFor(x => x.Caption)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");
    }
}
