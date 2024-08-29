using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Update;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure.Internal;
using Pomelo.EntityFrameworkCore.MySql.Migrations;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Helpers;

namespace ShardingCore.MultiField.Demo;

public class ShardingMySqlMigrationsSqlGenerator(
    MigrationsSqlGeneratorDependencies dependencies,
    ICommandBatchPreparer commandBatchPreparer,
#pragma warning disable EF1001
    IMySqlOptions options,
#pragma warning restore EF1001
    IShardingRuntimeContext shardingRuntimeContext)
    : MySqlMigrationsSqlGenerator(dependencies, commandBatchPreparer, options)
{
    protected override void Generate(MigrationOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        var oldMigrationCommands = builder.GetCommandList().ToList();
        base.Generate(operation, model, builder);
        var newMigrationCommands = builder.GetCommandList().ToList();
        
        var migrationCommands = 
            newMigrationCommands.Where(x => !oldMigrationCommands.Contains(x)).ToList();
        MigrationHelper.Generate(shardingRuntimeContext, operation, builder, Dependencies.SqlGenerationHelper,
            migrationCommands);
    }
}