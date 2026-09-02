using System.Reflection;

namespace OkVolleyVibes.Application;

/// <summary>Anchor type for assembly scanning and architecture tests.</summary>
public static class ApplicationAssemblyReference
{
    public static Assembly Assembly => typeof(ApplicationAssemblyReference).Assembly;
}
