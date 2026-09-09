namespace OkVolleyVibes.Domain.Players;

/// <summary>Lifecycle of a player's club membership. Transitions are defined in FR-C (not yet scoped).</summary>
public enum MembershipStatus
{
    Pending = 0,
    Active = 1,
    Lapsed = 2,
    Inactive = 3,
}
