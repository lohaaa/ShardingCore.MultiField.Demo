using System.Collections.Concurrent;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions;

namespace ShardingCore.MultiField.Demo;

public class MultiFieldRoute : AbstractShardingOperatorVirtualTableRoute<Player, string>
{

    private static readonly ConcurrentDictionary<string, object?> Tails = new(StringComparer.OrdinalIgnoreCase);

    public override string ShardingKeyToTail(object shardingKey)
    {
        var s = shardingKey.ToString();
        ArgumentException.ThrowIfNullOrWhiteSpace(s, nameof(shardingKey));
        return s;
    }

    public override List<string> GetTails()
    {
        return Tails.Keys.ToList();
    }

    public override void Configure(EntityMetadataTableBuilder<Player> builder)
    {
        builder.ShardingProperty(x => x.SplitTableKey);
        builder.ShardingExtraProperty(x => x.AppCode);
        builder.ShardingExtraProperty(x => x.GroupCode);
    }

    public override Func<string, bool> GetRouteToFilter(string shardingKey, ShardingOperatorEnum shardingOperator)
    {
        return tail => true;
    }
}