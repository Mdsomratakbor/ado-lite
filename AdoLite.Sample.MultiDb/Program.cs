using System.Data;
using AdoLite.Core.Base;
using AdoLite.Core.Enums;
using AdoLite.Core.Interfaces;
using AdoLite.Extension;
using AdoLite.Sample.MultiDb.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Multi-DB sample: configure SQL Server, PostgreSQL, and MySQL at once and query each.

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddSimpleConsole());

// Pull connections from env vars; fallback to local defaults if not set.
var sqlConn = Environment.GetEnvironmentVariable("ADOLITE_SQLSERVER_CONNECTION")
             ?? "Server=.;Database=TestDb;User Id=sa;Password=007;TrustServerCertificate=True;Pooling=true;Max Pool Size=50;";
var sqlReportingConn = Environment.GetEnvironmentVariable("ADOLITE_SQLSERVER_REPORTING")
                     ?? "Server=.;Database=ReportingDb;User Id=sa;Password=007;TrustServerCertificate=True;Pooling=true;Max Pool Size=50;";

var pgConn = Environment.GetEnvironmentVariable("ADOLITE_POSTGRES_CONNECTION")
            ?? "Host=localhost;Database=postgres;Username=postgres;Password=postgres;Pooling=true;Maximum Pool Size=50;";

var myConn = Environment.GetEnvironmentVariable("ADOLITE_MYSQL_CONNECTION")
            ?? "Server=localhost;Database=mysql;Uid=root;Pwd=root;Pooling=true;MaximumPoolSize=50;";

var settings = new DatabaseSettings();
settings.Connections["SqlServer"] = new DatabaseConfig
{
    Provider = DatabaseProvider.SqlServer,
    ConnectionString = sqlConn
};
settings.Connections["SqlServer.Reporting"] = new DatabaseConfig
{
    Provider = DatabaseProvider.SqlServer,
    ConnectionString = sqlReportingConn
};
settings.Connections["Postgres"] = new DatabaseConfig
{
    Provider = DatabaseProvider.PostgreSQL,
    ConnectionString = pgConn
};
settings.Connections["MySql"] = new DatabaseConfig
{
    Provider = DatabaseProvider.MySQL,
    ConnectionString = myConn
};

services.AddAdoLiteDataContexts(settings);

using var provider = services.BuildServiceProvider();
var factory = provider.GetRequiredService<IDataContextFactory>();

// Create contexts for each DB
using var sqlCtx = factory.Create("SqlServer");
using var sqlReportingCtx = factory.Create("SqlServer.Reporting");
using var pgCtx = factory.Create("Postgres");
using var myCtx = factory.Create("MySql");

User ReadUser(IDataContext ctx, string query, Dictionary<string, string>? parameters = null)
{
    return ctx.GetSingleRecord<User>(query, parameters);
}

var sqlUser = ReadUser(sqlCtx, "SELECT TOP 1 Id, Name FROM Users");
var reportCount = sqlReportingCtx.GetCount("SELECT COUNT(*) FROM ReportItems");
var pgUser = ReadUser(pgCtx, "SELECT id, name FROM users LIMIT 1");
var myUser = ReadUser(myCtx, "SELECT Id, Name FROM Users LIMIT 1");

Console.WriteLine($"SQL Server (primary): {sqlUser?.Name}, SQL Server (reporting) count: {reportCount}, Postgres: {pgUser?.Name}, MySQL: {myUser?.Name}");
