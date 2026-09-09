using FluentValidation;
using OkVolleyVibes.Application.Common.Abstractions;

namespace OkVolleyVibes.Application.Account.UploadPhoto;

internal sealed class UploadPhotoValidator : AbstractValidator<UploadPhotoCommand>
{
    private static readonly HashSet<string> AllowedContentTypes =
        new(PhotoLimits.AllowedContentTypes, StringComparer.OrdinalIgnoreCase);

    public UploadPhotoValidator(ITranslator t)
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage(t["Photo.Required"])
            .Must(content => content.Length <= PhotoLimits.MaxBytes).WithMessage(t["Photo.TooLarge"]);

        RuleFor(x => x.ContentType)
            .Must(AllowedContentTypes.Contains).WithMessage(t["Photo.UnsupportedType"]);

        RuleFor(x => x.Content)
            .Must(content => LooksLikeImage(content)).WithMessage(t["Photo.NotAnImage"])
            .When(x => x.Content is { Length: > 0 });
    }

    private static bool LooksLikeImage(ReadOnlySpan<byte> b)
    {
        if (b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF)
        {
            return true; // JPEG
        }

        if (b.Length >= 8 && b is [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, ..])
        {
            return true; // PNG
        }

        if (b.Length >= 12
            && b[..4].SequenceEqual("RIFF"u8)
            && b.Slice(8, 4).SequenceEqual("WEBP"u8))
        {
            return true; // WebP
        }

        return false;
    }
}
