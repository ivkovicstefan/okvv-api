using System.Reflection;

namespace OkVolleyVibes.Infrastructure;

/// <summary>Anchor type for assembly scanning and architecture tests.</summary>
public static class InfrastructureAssemblyReference
{
    public static Assembly Assembly => typeof(InfrastructureAssemblyReference).Assembly;
}
