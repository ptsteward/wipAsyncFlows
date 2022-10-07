using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using Xunit.Abstractions;

namespace PulsarFlows.Tests;

public class UnitTest1
{
    private readonly ITestOutputHelper output;

    public UnitTest1(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public async Task OneChannel_TwoRead_OneWrite()
    {
        var set1 = new List<Item>();
        var set2 = new List<Item>();
        var ch1 = Channel.CreateUnbounded<Item>();

        var read1 = Read(ch1.Reader);
        var read2 = Read(ch1.Reader);
        var write1 = Write(ch1.Writer);
        await Task.WhenAll(write1);

        set1 = await read1;
        set2 = await read2;

        output.WriteLine(@$"
set1: {set1.Count} 
set2: {set2.Count} 
");

        Assert.NotEmpty(set1);
        Assert.NotEmpty(set2);
        Assert.Equal(set1, set2, new Item(-1));
        Assert.Equal(set1, set2, new IdComparer());
    }

    [Fact]
    public async Task OneChannel_TwoRead_TwoWrite()
    {
        var set1 = new List<Item>();
        var set2 = new List<Item>();
        var ch1 = Channel.CreateUnbounded<Item>();

        var read1 = Read(ch1.Reader);
        var read2 = Read(ch1.Reader);
        var write1 = Write(ch1.Writer);
        var write2 = Write(ch1.Writer);
        await Task.WhenAll(write1, write2);

        set1 = await read1;
        set2 = await read2;

        output.WriteLine(@$"
set1: {set1.Count} 
set2: {set2.Count} 
");

        Assert.NotEmpty(set1);
        Assert.NotEmpty(set2);
        Assert.Equal(set1, set2, new Item(-1));
        Assert.Equal(set1, set2, new IdComparer());
    }

    [Fact]
    public async Task TwoChannel_TwoRead_OneWrite()
    {
        var set1 = new List<Item>();
        var set2 = new List<Item>();
        var ch1 = Channel.CreateUnbounded<Item>();
        var ch2 = Channel.CreateUnbounded<Item>();

        var read1 = Read(ch1.Reader);
        var read2 = Read(ch2.Reader);
        var write1 = Write(ch1.Writer);
        await Task.WhenAll(write1);

        set1 = await read1;
        set2 = await read2;

        output.WriteLine(@$"
set1: {set1.Count} 
set2: {set2.Count}
");

        Assert.NotEmpty(set1);
        Assert.NotEmpty(set2);
        Assert.Equal(set1, set2, new Item(-1));
        Assert.Equal(set1, set2, new IdComparer());
    }

    [Fact]
    public async Task TwoChannel_TwoRead_TwoWrite()
    {
        var set1 = new List<Item>();
        var set2 = new List<Item>();
        Assert.NotEmpty(set1);
        Assert.NotEmpty(set2);
        var ch1 = Channel.CreateUnbounded<Item>();
        var ch2 = Channel.CreateUnbounded<Item>();

        var read1 = Read(ch1.Reader);
        var read2 = Read(ch2.Reader);
        var write1 = Write(ch1.Writer);
        var write2 = Write(ch2.Writer);
        await Task.WhenAll(write1, write2);

        set1 = await read1;
        set2 = await read2;

        output.WriteLine(@$"
set1: {set1.Count} 
set2: {set2.Count}
");

        Assert.NotEmpty(set1);
        Assert.NotEmpty(set2);
        Assert.Equal(set1, set2, new Item(-1));
        Assert.Equal(set1, set2, new IdComparer());
    }

    public async Task<List<Item>> Read(ChannelReader<Item> reader)
    {
        await Task.Yield();
        var set = new List<Item>();
        while (!reader.Completion.IsCompleted)
        {
            await foreach (var item in reader.ReadAllAsync())
            {
                set.Add(item);
            }
        }
        return set;
    }

    public async Task Write(ChannelWriter<Item> writer)
    {
        await Task.Yield();
        foreach (var i in Enumerable.Range(0, 1_000).Select(x => Random.Shared.Next(1, 10_000)))
        {
            var item = new Item(i);
            await writer.WriteAsync(item);
            await Task.Delay(10);                       
        }
        writer.Complete();
    }
}

public record Item : IEqualityComparer<Item>
{
    public int Number { get; }
    public Guid Id { get; } = Guid.NewGuid();

    public Item(int i) => Number = i;

    public bool Equals(Item? x, Item? y) => x?.Number == y?.Number;
    public int GetHashCode([DisallowNull] Item obj) => obj.Number.GetHashCode();
}

public class IdComparer : IEqualityComparer<Item>
{
    public bool Equals(Item? x, Item? y) => x?.Id == y?.Id;
    public int GetHashCode([DisallowNull] Item obj) => obj.Id.GetHashCode();
}
