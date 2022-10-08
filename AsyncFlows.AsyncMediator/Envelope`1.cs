namespace AsyncFlows.AsyncMediator;

public sealed record Envelope<TPayload>(
    TPayload Payload,
    string CurrentId,
    string CausationId)
    : Envelope(CurrentId, CausationId);
