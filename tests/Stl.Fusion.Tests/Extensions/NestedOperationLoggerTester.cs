using MemoryPack;
using Stl.Fusion.Extensions;

namespace Stl.Fusion.Tests.Extensions;

public class NestedOperationLoggerTester : IComputeService
{
    [DataContract, MemoryPackable]
    public partial record SetManyCommand(
        [property: DataMember] string[] Keys,
        [property: DataMember] string ValuePrefix
    ) : ICommand<Unit>;

    private IKeyValueStore KeyValueStore { get; }

    public NestedOperationLoggerTester(IKeyValueStore keyValueStore)
        => KeyValueStore = keyValueStore;

    [CommandHandler]
    public virtual async Task SetMany(SetManyCommand command, CancellationToken cancellationToken = default)
    {
        var (keys, valuePrefix) = command;
        var first = keys.FirstOrDefault();
        if (first == null)
            return;
        await KeyValueStore.Set(default, first, valuePrefix + keys.Length, cancellationToken);
        var nextCommand = new SetManyCommand(keys[1..], valuePrefix);
        var commander = this.GetCommander();
        await commander.Call(nextCommand, cancellationToken).ConfigureAwait(false);
    }
}
