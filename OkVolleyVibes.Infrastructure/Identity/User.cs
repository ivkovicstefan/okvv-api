using Microsoft.AspNetCore.Identity;

namespace OkVolleyVibes.Infrastructure.Identity;

/// <summary>
/// The identity/auth record plus basic profile. Volleyball and payment data live on the domain
/// <c>PlayerProfile</c> (1:1), not here. Identity is an infrastructure concern, so this type stays
/// in Infrastructure and the Application layer reaches it only through <c>IIdentityService</c>.
/// </summary>
public sealed class User : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    /// <summary>BCP-47 tag: <c>en</c>, <c>sr-Latn</c> or <c>ru</c>. Drives localized emails.</summary>
    public string PreferredLanguage { get; set; } = "en";

    public DateOnly? DateOfBirth { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Set once the user finishes the onboarding survey (FR-A11). Null = onboarding pending.</summary>
    public DateTime? ProfileCompletedAtUtc { get; set; }
}
