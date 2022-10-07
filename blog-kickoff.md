# Inspiration
## ***Not YAM*** 
### ***Yet Another Mediator***
There's already well established and widely used mediator packages e.g. [MediatR](https://github.com/jbogard/MediatR), [Mediator.NET](https://mayuanyang.github.io/Mediator.Net/), etc.
These all quite well implement variations on the [Mediator Pattern...](https://en.wikipedia.org/wiki/Mediator_pattern)🤷‍♂️. Largely we see these operating in process and not across the wire, with the exception of [MassTransit - Mediator](https://masstransit-project.com/usage/mediator.html) of course, but we won't go there right now.😎
## What's a Mediator
Traditionally, a Mediator represents a focal point point between an `Originator` and one or many `Consumers`

So why create another? Well let's look closer; all these options are quite feature rich, so to speak, and support much the same patterns
* *CQRS Style Patterns*
    * [Query, Request/Response/(Query)Handler](https://github.com/jbogard/MediatR/wiki#requestresponse)
    * [Command/CommandHandler](https://github.com/mayuanyang/Mediator.Net#sending-a-command-with-no-response)
* *Pub/Sub*
    * [Notifications](https://github.com/jbogard/MediatR/wiki#notifications)
    * [Publish Events](https://github.com/mayuanyang/Mediator.Net#publishing-an-event)
* *Pipelines / Middleware / Behaviors*
    * [Pipelining & Behaviors](https://jimmybogard.com/sharing-context-in-mediatr-pipelines/)
    * [Pipeline Behaviors](https://garywoodfine.com/how-to-use-mediatr-pipeline-behaviours/)
    * [Mediator.NET Pipelining](https://github.com/mayuanyang/Mediator.Net#using-pipelines)
    * [Mediator.NET Middleware](https://github.com/mayuanyang/Mediator.Net#setting-up-middlewares)
## What's a Mediator for?

# Schema Driven Mediator
## Conceptually
Conceptually what we're after is exactly as you'd find in any distributed messaging system: Kafka, Pulsar, RabbitMQ, Servicebus, etc. Most current offerrings of in process *Mediators* tend to couple the **Origination** with the **Consumption**; this is the key I want changed.
## Primary Goals
Fundamentally we want an entirely deccoupled architecture. The only shared knowldge should be the message `Schema`
1. Decoupling between processing
    * The `Originating Process` should remain separate from the `Consuming Process`
1. Declared Consumption - *this is where we decouple*
    * No concept of *registering* ***`Handlers`***
    * Yes, I have a distaste for ***`Handlers`***
## Secondary Goals
These arise as result of our *Primary Goals*
1. Broadcast & Multicast Origination
    * One or Many `Originators` can produce a message
    * One or Many `Consumers` can listen
1. Declared expectation of Result
    * Broadcast/Multicast still applies
    * One or Many Consumers can provide a Result
