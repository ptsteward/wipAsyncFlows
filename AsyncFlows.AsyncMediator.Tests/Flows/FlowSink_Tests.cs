using AsyncFlows.AsyncMediator.Tests.Utilities;
using AsyncFlows.Extensions;
using AsyncFlows.Registrations;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AsyncFlows.AsyncMediator.Tests.Flows;

public class Flow_UnitTests : IAsyncDisposable
{
    private readonly ITestOutputHelper console;
    private readonly IFlowSource<Envelope<string>> source;
    private readonly IFlowSink<Envelope<string>> sink;
    private readonly CancellationTokenSource tokenSource;
    private readonly CancellationToken cancelToken;

    public Flow_UnitTests(ITestOutputHelper console)
    {
        var services = new ServiceCollection();
        services.AddFlow<Envelope<string>>();
        var provider = services.BuildServiceProvider();

        this.source = provider.GetRequiredService<IFlowSource<Envelope<string>>>();
        this.sink = provider.GetRequiredService<IFlowSink<Envelope<string>>>();
        this.tokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
        this.cancelToken = tokenSource.Token;
        this.console = console.NotNull();
    }

    [Theory, AutoMoqData]
    public async Task Flow_SinkLinksToSource_ConsumesExpected(
        Envelope<string> expected)
    {
        var submitted = await source.EmitAsync(expected, cancelToken);
        Assert.True(submitted);

        var actual = await sink.ConsumeAsync(cancelToken).SingleAsync();
        Assert.Equal(expected, actual);
    }

    [Theory, AutoMoqData]
    public async Task Flow_CompletionPropagation_SinkStopsEnumeration_Graceful(
        Envelope<string> expected)
    {
        var set = await EnumerateSink(expected);

        set.Should().ContainSingle()
            .And.Contain(expected);
    }

    [Theory, AutoMoqData]
    public async Task Flow_CompletionPropagation_SinkIsDisposed(
        Envelope<string> expected)
    {
        var set = await EnumerateSink(expected);
        set.Should().ContainSingle();

        await sink.Awaiting(_ => EnumerateSink(expected))
            .Should()
            .ThrowAsync<ObjectDisposedException>();
    }

    private async Task<IEnumerable<Envelope<string>>> EnumerateSink(
        Envelope<string> expected)
    {
        await using (var shortSource = source)
        {
            await shortSource.EmitAsync(expected, cancelToken);
            var set = new List<Envelope<string>>();
            await foreach (var msg in sink.ConsumeAsync(cancelToken))
                set.Add(msg);
            return set;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await source.DisposeAsync();
        await sink.DisposeAsync();
    }
}
