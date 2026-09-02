using System.Reflection;

namespace OkVolleyVibes.Domain;

/// <summary>Anchor type for assembly scanning and architecture tests.</summary>
public static class DomainAssemblyReference
{
    public static Assembly Assembly => typeof(DomainAssemblyReference).Assembly;
}
