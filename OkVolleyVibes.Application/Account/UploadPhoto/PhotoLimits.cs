namespace OkVolleyVibes.Application.Account.UploadPhoto;

public static class PhotoLimits
{
    public const int MaxBytes = 512 * 1024;

    public static readonly IReadOnlyList<string> AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];
}
