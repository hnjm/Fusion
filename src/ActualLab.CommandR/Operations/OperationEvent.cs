namespace ActualLab.CommandR.Operations;

public sealed record OperationEvent(
    Symbol Uuid,
    Moment LoggedAt,
    Moment FiresAt,
    object? Value
    ) : IHasUuid, IHasId<Symbol>
{
    Symbol IHasId<Symbol>.Id => Uuid;

    // Computed
    public bool HasFiresAt => FiresAt != default;
}
