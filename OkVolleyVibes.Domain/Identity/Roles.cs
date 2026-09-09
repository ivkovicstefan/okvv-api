namespace OkVolleyVibes.Domain.Identity;

/// <summary>
/// The fixed, seeded role set. Developers extend this list; end users never create roles.
/// Anything finer ("coach of team X") is a scoped permission (policy + claim), not a role.
/// </summary>
public static class Roles
{
    public const string Ceo = "CEO";
    public const string FinanceManager = "FinanceManager";
    public const string Coach = "Coach";
    public const string RecreationCoordinator = "RecreationCoordinator";
    public const string Player = "Player";

    /// <summary>All roles, most privileged first.</summary>
    public static readonly IReadOnlyList<string> All =
    [
        Ceo,
        FinanceManager,
        Coach,
        RecreationCoordinator,
        Player,
    ];
}
