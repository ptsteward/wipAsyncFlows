using System.Threading.Tasks.Dataflow;

namespace AsyncFlows.AsyncMediator;

internal interface ILinkableSource<TItem>
    where TItem : Envelope
{
    IDisposable LinkTo(ITargetBlock<TItem> sink);
}
