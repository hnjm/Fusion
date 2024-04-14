using ActualLab.Fusion.EntityFramework.LogProcessing;
using ActualLab.Fusion.EntityFramework.Operations;
using ActualLab.Redis;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace ActualLab.Fusion.EntityFramework.Redis;

public class RedisDbLogWatcher<TDbContext, TDbEntry>(
    RedisDbLogWatcherOptions<TDbContext> settings,
    IServiceProvider services
    ) : DbLogWatcher<TDbContext, TDbEntry>(services)
    where TDbContext : DbContext
{
    protected RedisDb RedisDb { get; }
        = services.GetService<RedisDb<TDbContext>>() ?? services.GetRequiredService<RedisDb>();

    public RedisDbLogWatcherOptions<TDbContext> Settings { get; } = settings;

    protected override DbShardWatcher CreateShardWatcher(DbShard shard)
        => new ShardWatcher(this, shard);

    // Nested types

    protected class ShardWatcher : DbShardWatcher
    {
        public RedisDbLogWatcher<TDbContext, TDbEntry> Owner { get; }
        public string Key { get; }
        public RedisPub RedisPub { get; }
        public RedisValue NotifyPayload { get; }

        public ShardWatcher(RedisDbLogWatcher<TDbContext, TDbEntry> owner, DbShard shard) : base(shard)
        {
            Owner = owner;
            var hostId = owner.DbHub.HostId;
            Key = owner.Settings.PubSubKeyFormatter.Invoke(Shard, typeof(TDbEntry));
            RedisPub = owner.RedisDb.GetPub(Key);
            NotifyPayload = "";
            Owner.Log.IfEnabled(LogLevel.Debug)
                ?.LogDebug("Watch[{Shard}]: pub/sub key = '{Key}'", shard, RedisPub.FullKey);

            var watchChain = new AsyncChain($"Watch({shard})", async cancellationToken => {
                var redisSub = owner.RedisDb.GetChannelSub(Key);
                await using var _ = redisSub.ConfigureAwait(false);

                await redisSub.Subscribe().ConfigureAwait(false);
                while (!cancellationToken.IsCancellationRequested) {
                    var value = await redisSub.Messages
                        .ReadAsync(cancellationToken)
                        .ConfigureAwait(false);
                    if (!StringComparer.Ordinal.Equals(hostId.Id.Value, value))
                        MarkChanged();
                }
            }).RetryForever(owner.Settings.WatchRetryDelays, owner.Log);

            _ = watchChain.RunIsolated(StopToken);
        }

        public override Task NotifyChanged(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return RedisPub.Publish(NotifyPayload);
        }
    }
}
