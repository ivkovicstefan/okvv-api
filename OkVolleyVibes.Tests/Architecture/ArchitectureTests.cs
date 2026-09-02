using NetArchTest.Rules;
using OkVolleyVibes.Application;
using OkVolleyVibes.Domain;
using OkVolleyVibes.Infrastructure;

namespace OkVolleyVibes.Tests.Architecture;

/// <summary>Guards the Clean Architecture dependency rules. See the `dotnet-api` skill.</summary>
public sealed class ArchitectureTests
{
    private const string Domain = "OkVolleyVibes.Domain";
    private const string Application = "OkVolleyVibes.Application";
    private const string Infrastructure = "OkVolleyVibes.Infrastructure";
    private const string Api = "OkVolleyVibes.Api";
    private const string Mediator = "OkVolleyVibes.Mediator";

    [Fact]
    public void Domain_should_not_depend_on_other_layers()
    {
        TestResult result = Types.InAssembly(DomainAssemblyReference.Assembly)
            .That().ResideInNamespace(Domain)
            .ShouldNot().HaveDependencyOnAny(Application, Infrastructure, Api, Mediator)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_should_not_depend_on_infrastructure_or_api()
    {
        TestResult result = Types.InAssembly(ApplicationAssemblyReference.Assembly)
            .That().ResideInNamespace(Application)
            .ShouldNot().HaveDependencyOnAny(Infrastructure, Api)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Infrastructure_should_not_depend_on_api()
    {
        TestResult result = Types.InAssembly(InfrastructureAssemblyReference.Assembly)
            .That().ResideInNamespace(Infrastructure)
            .ShouldNot().HaveDependencyOnAny(Api)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
