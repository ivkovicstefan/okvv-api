using Microsoft.Extensions.DependencyInjection;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Tests.Mediator;

public sealed class AddMediatorTests
{
    [Fact]
    public async Task Scans_and_registers_request_handlers_from_the_given_assembly()
    {
        var services = new ServiceCollection();
        services.AddMediator(typeof(AddMediatorTests).Assembly);

        ISender sender = services.BuildServiceProvider().CreateScope()
            .ServiceProvider.GetRequiredService<ISender>();

        int result = await sender.Send(new Doubling(21));

        result.Should().Be(42);
    }

    [Fact]
    public void Throws_when_no_assembly_is_supplied()
    {
        Action act = () => new ServiceCollection().AddMediator();

        act.Should().Throw<ArgumentException>();
    }

    public sealed record Doubling(int Value) : IRequest<int>;

    public sealed class DoublingHandler : IRequestHandler<Doubling, int>
    {
        public Task<int> Handle(Doubling request, CancellationToken ct) => Task.FromResult(request.Value * 2);
    }
}
