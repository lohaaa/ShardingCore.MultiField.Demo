using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using MySqlConnector;
using Pomelo.EntityFrameworkCore.MySql.Migrations;

namespace ShardingCore.MultiField.Demo;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        // builder.Services.AddDbContext<ApplicationDbContext>(options =>
        // {
        //     options.UseMySql(new MySqlConnection(builder.Configuration.GetConnectionString("Default")), MySqlServerVersion.LatestSupportedServerVersion);
        // });
        builder.Services.AddShardingDbContext<ApplicationDbContext>()
            .UseRouteConfig(
                options =>
                    options.AddShardingTableRoute<MultiFieldRoute>()
            )
            .UseConfig(options =>
            {
                options.CheckShardingKeyValueGenerated = false;
                options.AddDefaultDataSource("Default", builder.Configuration.GetConnectionString("Default"));
                options.UseShardingQuery((s, optionsBuilder) =>
                {
                    optionsBuilder.UseMySql(s, MySqlServerVersion.LatestSupportedServerVersion);
                });
                options.UseShardingTransaction((connection, optionsBuilder) =>
                {
                    optionsBuilder.UseMySql(connection, MySqlServerVersion.LatestSupportedServerVersion);
                });
                options.UseShardingMigrationConfigure(optionsBuilder =>
                    optionsBuilder.ReplaceService<IMigrationsSqlGenerator, MySqlMigrationsSqlGenerator>());
            }).AddShardingCore();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}