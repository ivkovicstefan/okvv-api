namespace OkVolleyVibes.Infrastructure.Configuration;

/// <summary>Bound from the <c>Seed</c> configuration section. Only honoured in Development.</summary>
public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public bool Enabled { get; set; }

    public List<SeedUser> Users { get; set; } = [];
}

public sealed class SeedUser
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = [];
}
