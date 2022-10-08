using System.Threading.Tasks.Dataflow;

namespace AsyncFlows.AsyncMediator.Flows;

internal interface ILinkableSource<TSchema>
    where TSchema : Envelope
{
    IDisposable LinkTo(ITargetBlock<TSchema> sink);
}
