using Microsoft.Extensions.DependencyInjection;
using OkVolleyVibes.Mediator;

namespace OkVolleyVibes.Tests.Mediator;

public sealed class SenderTests
{
    [Fact]
    public async Task Dispatches_to_the_registered_handler()
    {
        ISender sender = BuildSender(services =>
            services.AddScoped<IRequestHandler<Echo, string>, EchoHandler>());

        string result = await sender.Send(new Echo("hi"));

        result.Should().Be("echo:hi");
    }

    [Fact]
    public async Task Runs_behaviors_outermost_first_then_handler()
    {
        var log = new List<string>();
        ISender sender = BuildSender(services =>
        {
            services.AddSingleton(log);
            services.AddScoped<IRequestHandler<Echo, string>, LoggingEchoHandler>();
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(OuterBehavior<,>));
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(InnerBehavior<,>));
        });

        await sender.Send(new Echo("x"));

        log.Should().Equal("outer:before", "inner:before", "handler", "inner:after", "outer:after");
    }

    [Fact]
    public async Task A_behavior_can_short_circuit_before_the_handler()
    {
        var log = new List<string>();
        ISender sender = BuildSender(services =>
        {
            services.AddSingleton(log);
            services.AddScoped<IRequestHandler<Echo, string>, LoggingEchoHandler>();
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ShortCircuitBehavior<,>));
        });

        Func<Task> act = () => sender.Send(new Echo("x"));

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("blocked");
        log.Should().BeEmpty();
    }

    [Fact]
    public async Task Throws_a_clear_error_when_no_handler_is_registered()
    {
        ISender sender = BuildSender(_ => { });

        Func<Task> act = () => sender.Send(new Echo("x"));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No handler registered*Echo*");
    }

    private static ISender BuildSender(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        services.AddScoped<ISender, Sender>();
        configure(services);
        return services.BuildServiceProvider().CreateScope().ServiceProvider.GetRequiredService<ISender>();
    }

    private sealed record Echo(string Text) : IRequest<string>;

    private sealed class EchoHandler : IRequestHandler<Echo, string>
    {
        public Task<string> Handle(Echo request, CancellationToken ct) => Task.FromResult($"echo:{request.Text}");
    }

    private sealed class LoggingEchoHandler(List<string> log) : IRequestHandler<Echo, string>
    {
        public Task<string> Handle(Echo request, CancellationToken ct)
        {
            log.Add("handler");
            return Task.FromResult("ok");
        }
    }

    private sealed class OuterBehavior<TRequest, TResponse>(List<string> log)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            log.Add("outer:before");
            TResponse response = await next(ct);
            log.Add("outer:after");
            return response;
        }
    }

    private sealed class InnerBehavior<TRequest, TResponse>(List<string> log)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        {
            log.Add("inner:before");
            TResponse response = await next(ct);
            log.Add("inner:after");
            return response;
        }
    }

    private sealed class ShortCircuitBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
            => throw new InvalidOperationException("blocked");
    }
}
