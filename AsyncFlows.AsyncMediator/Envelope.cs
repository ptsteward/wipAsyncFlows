namespace AsyncFlows.AsyncMediator;

public abstract record Envelope(
    string CurrentId,
    string CausationId);
