namespace OkVolleyVibes.Domain.Players;

/// <summary>Playing positions. A player may hold more than one, so this is a bit flag.</summary>
[Flags]
public enum VolleyballPosition
{
    None = 0,
    Setter = 1,
    OutsideHitter = 2,
    Opposite = 4,
    MiddleBlocker = 8,
    Libero = 16,
}
