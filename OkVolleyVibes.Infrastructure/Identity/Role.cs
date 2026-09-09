using Microsoft.AspNetCore.Identity;

namespace OkVolleyVibes.Infrastructure.Identity;

public sealed class Role : IdentityRole<Guid>
{
    public Role()
    {
    }

    public Role(string roleName) : base(roleName)
    {
    }
}
