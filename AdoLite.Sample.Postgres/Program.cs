using System.Data;
using AdoLite.Core.Base;
using AdoLite.Core.Enums;
using AdoLite.Core.Interfaces;
using AdoLite.Extension;
using AdoLite.Sample.Postgres.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Sample console app to exercise all public methods against PostgreSQL.
// Replace the connection string with a real database before running.

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddSimpleConsole());

var settings = new DatabaseSettings();
settings.Connections["Default"] = new DatabaseConfig
{
    Provider = DatabaseProvider.PostgreSQL,
    ConnectionString = "Host=localhost;Database=sampledb;Username=postgres;Password=yourpassword;Pooling=true;Maximum Pool Size=50;"
};

services.AddAdoLiteDataContexts(settings);

using var provider = services.BuildServiceProvider();
var factory = provider.GetRequiredService<IDataContextFactory>();
var ctx = factory.Create("Default");

try
{
    // Read operations
    var firstUser = ctx.GetSingleRecord<User>("SELECT id, name FROM users LIMIT 1");
    var firstRow = ctx.GetDataRow("SELECT id, name FROM users LIMIT 1");
    var table = ctx.GetDataTable("SELECT id, name FROM users LIMIT 5");
    var dataSet = ctx.GetDataSet("SELECT id, name FROM users LIMIT 5; SELECT id FROM users LIMIT 5;", null);
    var name = ctx.GetSingleValue<string>("SELECT name FROM users WHERE id = @Id", new() { { "@Id", "1" } });
    var idList = ctx.GetList<int>("SELECT id FROM users LIMIT 5");
    var dict = ctx.GetDictionary<int, string>("SELECT id, name FROM users LIMIT 5");
    var mapped = ctx.GetMappedList("SELECT id, name FROM users LIMIT 5", row => new User
    {
        Id = row.Field<int>("id"),
        Name = row.Field<string>("name")
    });
    var exists = ctx.Exists("SELECT 1 FROM users WHERE id = @Id", new() { { "@Id", "1" } });
    var count = ctx.GetCount("SELECT COUNT(*) FROM users");
    var paged = ctx.GetPagedDataTable("SELECT id, name FROM users ORDER BY id", null, 1, 10);

    // Write operations (comment out if you don't want to mutate data)
    var insertPattern = new QueryPattern
    {
        Query = "INSERT INTO users (name) VALUES (@Name)",
        Parameters = new List<Dictionary<string, object>>
        {
            new() { { "@Name", "sampleuser" } }
        }
    };

    var saveResult = ctx.SaveChanges(new List<IQueryPattern> { insertPattern });
    Console.WriteLine($"SaveChanges completed: {saveResult}");

    Console.WriteLine($"First user: {firstUser?.Name}");
    Console.WriteLine($"Exists? {exists}, Count={count}, Rows in page={paged.Rows.Count}");
}
finally
{
    (ctx as IDisposable)?.Dispose();
}
