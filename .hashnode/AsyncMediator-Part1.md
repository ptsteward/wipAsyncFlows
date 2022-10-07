# Inspiration
> **Origination** lives distinctly from **Consumption**

## *From the beginning* 🤔
There's already well established and widely used mediator packages e.g. [MediatR](https://github.com/jbogard/MediatR); *J. Bogard*, [Mediator.NET](https://mayuanyang.github.io/Mediator.Net/); *E. Ma*, etc.
These all quite well implement variations on the [Mediator Pattern...](https://en.wikipedia.org/wiki/Mediator_pattern)👌. 
Largely we see these operating in process and not across the wire, with the exception of [MassTransit - Mediator](https://masstransit-project.com/usage/mediator.html) of course, but we won't go there right now.😎
Additionally we can see many of the fundamental ideas presented within the *[...Query Side](https://blogs.cuttingedge.it/steven/posts/2011/meanwhile-on-the-query-side-of-my-architecture/) and [... Command Side](https://blogs.cuttingedge.it/steven/posts/2011/meanwhile-on-the-command-side-of-my-architecture/) of my architecture;* *S. Deursen*, also of [Simple Injector](https://github.com/simpleinjector/SimpleInjector) Rock Stardom 🎸

## On the market today
Right up front; I really enjoy both these libraries but ultimately I'd like to go another direction 🤔

First, let's take a closer look; all these options are quite feature rich, and support similar architectural patterns/styles.

* *CQRS Style Patterns*
    * [Query/QueryHandler, Request/Response](https://github.com/jbogard/MediatR/wiki#requestresponse)
    * [Command/CommandHandler](https://github.com/mayuanyang/Mediator.Net#sending-a-command-with-no-response)
* *Pub/Sub*
    * [Notifications](https://github.com/jbogard/MediatR/wiki#notifications)
    * [Publish Events](https://github.com/mayuanyang/Mediator.Net#publishing-an-event)
* *Pipelines / Middleware / Behaviors*
    * [Pipelining & Behaviors](https://jimmybogard.com/sharing-context-in-mediatr-pipelines/)
    * [Pipeline Behaviors](https://garywoodfine.com/how-to-use-mediatr-pipeline-behaviours/)
    * [Mediator.NET Pipelining](https://github.com/mayuanyang/Mediator.Net#using-pipelines)
    * [Mediator.NET Middleware](https://github.com/mayuanyang/Mediator.Net#setting-up-middlewares)

## What about the Mediator?
Traditionally, a Mediator represents a focal point between an *Originator* and one or many *Consumers*. This allows the *Originator* to avoid explicitly referring to or knowing about the *Consumers*

![Blog - Async Mediator - Typical Mediator.png](https://cdn.hashnode.com/res/hashnode/image/upload/v1665074232468/89alJ7DoN.png align="center")

The *Consumers* are typically conceptualized as `Handlers` of particular messages and can form `Pipelines` or chains. Each `Handler` playing a specific role in the choreography. Additional and complimentary concepts can be added with associated `Middleware` or `Behaviors`, etc. promoting some aspects of loose coupling. But the call from *Originator* to `Handler` is still synchronous with regard to its `Registration`. The `Registration` couples, albeit in the background, the *Originator* and the *Consumer* both process are tightly working together via the `Mediator`.

But imagine if this *`Mediator`* lived outside our process and these `Handlers` could come and go as would any Pod within a K8 cluster?
The trouble I see is within the ***Registration*** of these `Handlers`.

For instance *MediatR* holds a reference to all of its *Handlers* and dispatches associated `Requests` and/or `Notifications`. *Mediator.Net* has a nice abstraction in place; `Pipes` form its central core and you can even stream responses back via the receive pipeline. 
`💘IAsyncEnumerable` 

However, all `Handlers` are registered and well known. Both of these methodologies yield a static topology.

# Schema Driven Mediator
## Conceptually
Conceptually what we're after is much the same as you'd find in a distributed messaging broker: Kafka, Pulsar, RabbitMQ, etc. However, most current offerings of in process *Mediators* tend to couple the **Origination** with the **Consumption**; 
> This is the key I want changed; **Origination** lives distinctly from **Consumption**

## Architectural Objectives
### Primary Goals
Fundamentally we want an entirely decoupled architecture; the only shared knowledge should be the message `Schema`.
1. Decoupling between processing units
    * Schema Driven communication
    * The `Originating Process` should remain separate from the `Consuming Process`
1. Consumer Declared Consumption - *this is where we decouple*
    * No concept of *registering* ***`Handlers`***
    * *Consumers* and their associated process *may come and go*
1. Declared expectation of a `Result`
    * The `Mediator` is told a `Result` is expected

### Implications - Secondary Goals
These are goals that arise as result of our *Primary Goals*
1. Broadcast & Multicast Origination
    * One or Many *Originators* can produce a message
    * One or Many *Consumers* can listen
1. Results still adhere to Broadcast/Multicast configuration
    * One or Many *Originators* can *request a `Result`*
    * One or Many *Consumers* can *provide a `Result`*
    * A `Result` may be unrequited 💔, i.e. never arrive

![Blog - Async Mediator - Outbound.png](https://cdn.hashnode.com/res/hashnode/image/upload/v1665123223164/2QBBGShDP.png align="center")

Above, we can see the type of outbound Broadcast/Multicast style brokerage we're after. A decoupled, fanout of *Messages*, is relatively easy to achieve and we'll focus on that first.

# Putting it together
## Foundations
Our first step is to split the *Originator* from `Consumer` and further avoid any implicit coupling. All `Requests`, `Commands`, `Notifications`, `Responses`, etc. will travel asynchronously. The building blocks of this system will be the abstract concepts of ***Schema***, ***Message***, and ultimately ***Flow***. 
* **Schema** - Defines the shape of data/payload/behavior to be implemented by a *Message*
* **Message** - Takes the form defined by the *Schema* and represents an *immutable and unique* representation of that *Schema* at a point in time
* **Flow** -  Is the unidirectional transmission of *Messages*. A single *Flow* only permits a single *Schema*

## Async Message Fanout
### Abstractions
Producing a *Flow* are our two fundamental building blocks: `FlowSource` & `FlowSink`. These complimentary components define either end of a *Flow* constrained to a single *Schema*. The *Messages* will be transmitted in the background of our application via a `Source`/`Sink` set of abstractions. Each is defined below:

```csharp
public interface IFlowSource<TItem>
    : IAsyncDisposable, IDisposable
    where TItem : Envelope
{
    ValueTask<bool> EmitAsync(TItem item, CancellationToken cancelToken = default);
}

public interface IFlowSink<TItem>
    : IAsyncDisposable, IDisposable
    where TItem : Envelope
{
    IAsyncEnumerable<TItem> ConsumeAsync(CancellationToken cancelToken = default);
}
``` 

Note, both ends of our unidirectional *Flow* are `(Async)Disposable`. The unidirectional nature of the *Flow* leads directly to two implications.
1. Disposing a `Sink`, disposes only that recipient endpoint
1. Disposing a `Source`, however, closes the entirety of the *Flow*

Here we've also defined a constraint of Envelope, as seen, this constraint, i.e our base *Schema*, ensures our *Messages* always have a `CurrentId` and a `CausationId`. All this means is that we know where we are now and where we came from.

```csharp
public abstract record Envelope(
    string CurrentId,
    string CausationId);
```

Simple `Message` passing via the twin concepts of Source/Sink allow the *Origination* and *Consumption* to be decoupled. Our `Source`, via an *Originator*, produces messages at one end and the `Sink` receives them into a *Consumer*. This takes advantage of much the same concepts of any *Message Bus* but applies an asynchronous distributed behavior within a single application.

![Blog - Async Mediator - Source-Sink.png](https://cdn.hashnode.com/res/hashnode/image/upload/v1665123251089/7kLsmOIyH.png align="center")

### Implementations
Our implementations are built upon the now standard [***System...Dataflow***](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library) namespace containing a wide variety of *Dataflow* primitives. This library is quite powerful in terms of message passing and parallel processing; complimenting the typical async/await paradigm. Other implementations are of course possible; 
* [A little RX - System.Reactive, et. al.](https://github.com/dotnet/reactive)
* [Rolling a bit with System.Threading.Channels](https://learn.microsoft.com/en-us/dotnet/core/extensions/channels)

Each has tradeoffs of course, the main driver toward *Dataflow* was the simplicity of separating *Message Passing* and *Message Consuming*. I don't want to force any *Consumer* into participating in any semantic *Observable Pipeline*, especially if it is instead better represented as a discrete step. *Flow* remains opt-in and can be as simple as injection of the `Sink` and reading from it.
*Channels* of course could underpin *Dataflow* but natively *Channels* are not intended to support any kind of Broadcast/Multicast.

### FlowSink<TItem>
To build our *Flow* first we'll look at the `Sink`. The purpose and intention of this component is to be injected via dependency injection into any `Consumer` that has an interest in the defined `Schema`. That is;  any defined *Consumer* that must take action upon receiving a `Message` with the defined `Schema`. I've collapsed most of the disposal for brevity; the key piece is disposing of the link back to the `Source`.

```csharp
internal sealed class FlowSink<TItem>
    : IFlowSink<TItem>
    where TItem : Envelope
{    
    private readonly IDisposable link;
    private volatile bool isDisposed;
    private BufferBlock<TItem>? Buffer { get; set; }

    public FlowSink(ILinkableSource<TItem> source)
    {
        Buffer = new(new()
        {
            EnsureOrdered = true,
            BoundedCapacity = DataflowBlockOptions.Unbounded
        });
        link = source.LinkTo(Buffer);
    }

    public IAsyncEnumerable<TItem> ConsumeAsync(CancellationToken cancelToken = default)
        => Buffer.ThrowIfDisposed(isDisposed)
            .ToAsyncEnumerable(cancelToken);

    public void Dispose() {/*...*/}
    public ValueTask DisposeAsync() {/*...*/}
    private void DisposeCore() {/*...*/}
}
```

Our `FlowSink` is exposing an internal `BufferBlock` bound to our `Schema`. We maintain a concrete implementation of `BufferBlock` with the intention of later using its `Count` but this could well be represented as a `IPropagatorBlock` until specificities are necessary. Next, the only item we're dependent on for construction is an `ILinkableSource<TItem>` defined as follows

```csharp
internal interface ILinkableSource<TItem>
    where TItem : Envelope
{
    IDisposable LinkTo(ITargetBlock<TItem> sink);
}
```

Keeping this interface `internal` allows us to keep this concept scoped within our package. This narrowly exposes the internal mechanism of *Linking* two *Dataflow Blocks* together. Once linked, each block will process messages as defined by its implementation and operate independently of any other within the bounds set by the creation and linking. 

Lastly we can see that the `Buffer` is consumed via a custom cancellable extension to `IAsyncEnumerable`. While not necessary for this, we've decorated the `CancellationToken` as the target of `[EnumeratorCancellation]` for broader use cases.

```csharp
internal static async IAsyncEnumerable<TItem> ToAsyncEnumerable<TItem>(
    this ISourceBlock<TItem> source,
    [EnumeratorCancellation] CancellationToken cancelToken)
{
    while (await source.OutputAvailableAsync(cancelToken))
        yield return await source.ReceiveAsync(cancelToken);
}
```

### FlowSource<TItem>
Next is our Broadcast/Multicast enabled `Source`. This is accomplished by exposing the capabilities of a `BroadcastBlock`. This block clones, in our case - returns, each message received and *Offers* it to each *Linked* block. The importance of *Offer* is such that if a *Linked* block cannot take the `Message`; that `Message` is then dropped, i.e. lost forever and for good. This leads to *Backpressure*, another high point for choosing *Dataflow* yet out of scope here, but we set all *Blocks* with an `UnboundedCapacity` for simplicity to begin. So `Source` can be implemented as such; again with collapsed disposal, we're both *Completing* and then *awaiting Completion* of the `Source` during cleanup.

```csharp
internal sealed class FlowSource<TItem>
    : IFlowSource<TItem>,
    ILinkableSource<TItem>
    where TItem : Envelope
{    
    private volatile bool isDisposed;
    private BroadcastBlock<TItem>? Source { get; set; }

    public FlowSource()
        => Source = new(item => item,
            new()
            {
                EnsureOrdered = true,
                BoundedCapacity = DataflowBlockOptions.Unbounded
            });

    public ValueTask<bool> EmitAsync(TItem item, CancellationToken cancelToken = default)
        => Source.ThrowIfDisposed(isDisposed)
            .SendAsync(item, cancelToken);

    public IDisposable LinkTo(ITargetBlock<TItem> sink)
        => Source.ThrowIfDisposed(isDisposed)
            .LinkTo(sink, new()
            {
                PropagateCompletion = true,
            });

    public void Dispose() {/*...*/}
    public async ValueTask DisposeAsync() {/*...*/}
    private async ValueTask DisposeAsyncCore() {/*...*/}
}
```

This implementation exposes `EmitAsync` and transforms the standard `Task<bool>` of the *Block* into a disposed protected and a bit more generic `ValueTask<bool>` via simple extensions. Additionally we separately implement the `internal ILinkableSource<TItem>` interface to connect any downstream `Sinks`. This exposes a disposed protected call into `.LinkTo` ensuring that *Completion* is propagated. With this configuration set; if the `Source` is disposed and thus *Completed* this information will flow down to all `Sinks` which will then exit any ongoing or new iteration upon the `Sink`.

### Registration
With just these two components we can achieve the first two Primary Goals, albeit with a little DI trickery
1. Decoupling between processing units
1. Consumer Declared Consumption

```csharp
public static IServiceCollection AddFlow<TSchema>(this IServiceCollection services)
    where TSchema : Envelope
{
    services.TryAddSingleton<IFlowSource<TSchema>, FlowSource<TSchema>>();
    services.TryAddTransient<IFlowSink<TSchema>>(sp =>
    {
        var source = sp.GetRequiredService<IFlowSource<TSchema>>();
        var linkable = source as ILinkableSource<TSchema>
            ?? throw new ArgumentException(
                $"Invalid FlowSource Registration. Source Type, {source.GetType().Name}, is not linkable");
        return new FlowSink<TSchema>(linkable);
    });
    return services;
}
```

**Broadcast/Multicast**

Registering the `FlowSource` as a singleton ensures any consumer of this interface is *`Emitting`* to the same `BroadcastBlock`. This allows one to many *Originators* to enter `Messages` into the flow.

**Declared Consumption**

Thanks to the leniency of the default Dependency Injection of dotnet, i.e. `IServiceCollection/IServiceProvider`; we can register our `Sink` as a *Transient*. Doing so may yield a captured dependency if the consuming service has a longer lifetime than our `Sink`. However, the advantage is that we ensure each Consumer receives a unique and linked instance of our `Sink`. In this variation we're leveraging the DI container to dynamically construct our topology. Assuming, 🤞, our *Consumers* dispose of the `Sink` properly it will be appropriately removed from the `Flow` topology.

# Part 1 - Closure
With just these two primary components, `FlowSource` & `FlowSink`, we have achieved a basic decoupling and fanout of `Messages`. We really don't even need a specific *`Mediator`* to do this. However, we're still left with one primary requirement open

* Declared expectation of a `Result`

What we have can only send messages in a sort of *fire-and-forget* style, albeit never actually forgotten within the framework, our *Originator* couldn't care less. But the main use case of a `Mediator` is always to get a response. This means we have to tackle getting a `Result` back to an *Originator*, but recall we want this to remain 
1. 🤯Unidirectional & async...? 
1. 🤬And it must be compliant with the Broadcast/Multicast nature of our `Flow`
1. …And we have to realize that it may not be the ***first*** *Consumer* of our `Message` that actually returns our declared `Result`⁉️
1. *Ultimately this means taking a synchronous request, HTTP/GQL/gRPC/Event/etc.*
    * *Processing it async*
    * *And pulling it all back together for a sync response*

💥…I suppose that's all best left for ***Part 2 - It's Alive***