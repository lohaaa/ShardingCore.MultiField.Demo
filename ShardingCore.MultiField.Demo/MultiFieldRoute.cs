using System.Collections.Concurrent;
using MySqlConnector;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualDatabase.VirtualDataSources;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions;
using ShardingCore.TableCreator;

namespace ShardingCore.MultiField.Demo;

public class MultiFieldRoute : AbstractShardingOperatorVirtualTableRoute<Player, string>
{

    private static readonly ConcurrentDictionary<string, object?> Tails = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();
    private readonly ILogger<MultiFieldRoute> _logger;
    private readonly IVirtualDataSource _virtualDataSource;
    private readonly IShardingTableCreator _tableCreator;

    private const string Tables = "Tables";
    private const string TableSchema = "TABLE_SCHEMA";
    private const string TableName = "TABLE_NAME";

    public MultiFieldRoute(ILogger<MultiFieldRoute> logger, IVirtualDataSource virtualDataSource, IShardingTableCreator tableCreator)
    {
        _logger = logger;
        _virtualDataSource = virtualDataSource;
        _tableCreator = tableCreator;
        InitPlayerTails();
    }

    private void InitPlayerTails()
    {
        using var connection = new MySqlConnection(_virtualDataSource.DefaultConnectionString);
        connection.Open();
        var database = connection.Database;
        using var dataTable = connection.GetSchema(Tables);
        for (var i = 0; i < dataTable.Rows.Count; i++)
        {
            var schema = dataTable.Rows[i][TableSchema];
            if (!database.Equals($"{schema}", StringComparison.OrdinalIgnoreCase)) continue;
            var tableName = dataTable.Rows[i][TableName].ToString() ?? string.Empty;
            if (!tableName.StartsWith(nameof(Player), StringComparison.OrdinalIgnoreCase))
                continue;
            //如果没有下划线那么需要CurrentTableName.Length有下划线就要CurrentTableName.Length+1
            var length = nameof(Player).Length + 1;
            if (tableName.Length > length)
            {
                Tails.TryAdd(tableName[length..], null);
            }
        }
    }

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

    public override Func<string, bool> GetRouteFilter(object shardingKey, ShardingOperatorEnum shardingOperator, string shardingPropertyName)
    {
        //debugger
        return base.GetRouteFilter(shardingKey, shardingOperator, shardingPropertyName);
    }

    public override Func<string, bool> GetRouteToFilter(string shardingKey, ShardingOperatorEnum shardingOperator)
    {
//         var t = ShardingKeyToTail(shardingKey);
//         if (!GetTails().Exists(x => x == t))
//         {
//             _tableCreator.CreateTable<Player>(_virtualDataSource.DefaultDataSourceName, t);
//             Tails.TryAdd(t, null);
//         }
//
//         switch (shardingOperator)
//         {
//             case ShardingOperatorEnum.Equal: return tail => tail == t;
//             default:
//             {
// #if DEBUG
//                 _logger.LogWarning("shardingOperator is not equal scan all table tail");
// #endif
//                 return _ => true;
//             }
//         }
        return tail => true;
    }

    public override Func<string, bool> GetExtraRouteFilter(object shardingKey, ShardingOperatorEnum shardingOperator, string shardingPropertyName)
    {
        var value = shardingKey.ToString();
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(shardingKey));
        return shardingPropertyName switch
        {
            nameof(Player.AppCode) => GetAppCodeExtraRouteFilter(value, shardingOperator),
            nameof(Player.GroupCode) => GetGroupCodeExtraRouteFilter(value, shardingOperator),
            _ => throw new NotImplementedException(shardingPropertyName)
        };
    }

    private Func<string, bool> GetGroupCodeExtraRouteFilter(string value, ShardingOperatorEnum shardingOperator)
    {
        return shardingOperator switch
        {
            ShardingOperatorEnum.Equal => tail => tail.EndsWith(value),
            _ => tail => true
        };
    }

    private Func<string, bool> GetAppCodeExtraRouteFilter(string value, ShardingOperatorEnum shardingOperator)
    {
        return shardingOperator switch
        {
            ShardingOperatorEnum.Equal => tail => tail.StartsWith(value),
            _ => tail => true
        };
    }
}