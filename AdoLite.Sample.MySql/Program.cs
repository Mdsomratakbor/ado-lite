using System.Data;
using AdoLite.Core.Base;
using AdoLite.Core.Enums;
using AdoLite.Core.Interfaces;
using AdoLite.Extension;
using AdoLite.Sample.MySql.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Sample console app to exercise all public methods against MySQL.
// Replace the connection string with a real database before running.

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddSimpleConsole());

var settings = new DatabaseSettings();
settings.Connections["Default"] = new DatabaseConfig
{
    Provider = DatabaseProvider.MySQL,
    ConnectionString = "Server=localhost;Database=sampledb;Uid=root;Pwd=yourpassword;Pooling=true;MaximumPoolSize=50;"
};

services.AddAdoLiteDataContexts(settings);

using var provider = services.BuildServiceProvider();
var factory = provider.GetRequiredService<IDataContextFactory>();
var ctx = factory.Create("Default");

try
{
    // Read operations
    var firstUser = ctx.GetSingleRecord<User>("SELECT Id, Name FROM Users LIMIT 1");
    var firstRow = ctx.GetDataRow("SELECT Id, Name FROM Users LIMIT 1");
    var table = ctx.GetDataTable("SELECT Id, Name FROM Users LIMIT 5");
    var dataSet = ctx.GetDataSet("SELECT Id, Name FROM Users LIMIT 5; SELECT Id FROM Users LIMIT 5;", null);
    var name = ctx.GetSingleValue<string>("SELECT Name FROM Users WHERE Id = @Id", new() { { "@Id", "1" } });
    var idList = ctx.GetList<int>("SELECT Id FROM Users LIMIT 5");
    var dict = ctx.GetDictionary<int, string>("SELECT Id, Name FROM Users LIMIT 5");
    var mapped = ctx.GetMappedList("SELECT Id, Name FROM Users LIMIT 5", row => new User
    {
        Id = row.Field<int>("Id"),
        Name = row.Field<string>("Name")
    });
    var exists = ctx.Exists("SELECT 1 FROM Users WHERE Id = @Id", new() { { "@Id", "1" } });
    var count = ctx.GetCount("SELECT COUNT(*) FROM Users");
    var paged = ctx.GetPagedDataTable("SELECT Id, Name FROM Users ORDER BY Id", null, 1, 10);

    // Write operations (comment out if you don't want to mutate data)
    var insertPattern = new QueryPattern
    {
        Query = "INSERT INTO Users (Name) VALUES (@Name)",
        Parameters = new List<Dictionary<string, object>>
        {
            new() { { "@Name", "SampleUser" } }
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
