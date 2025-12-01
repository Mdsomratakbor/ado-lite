# AdoLite

A lightweight ADO.NET helper library with provider-agnostic data access, connection pooling, and multi-database support (SQL Server, PostgreSQL, MySQL). Includes DI extensions, transactional/bulk helpers, logging hooks, and sample apps.

## Features
- Provider-agnostic interfaces (`IDataContext`, `IDataQuery`, `IDataTransaction`).
- Supports SQL Server, PostgreSQL, and MySQL via provider-specific projects.
- Connection pooling via underlying providers; per-call open/close for pool friendliness.
- Transactions and batch execution (`SaveChanges`) with parameter support (including TVPs for SQL Server).
- Bulk insert helpers for all providers.
- DI extensions for quick registration and multiple named data sources.
- Optional logging of SQL, parameters, duration, and errors.
- Sample projects (single-DB, multi-DB, Northwind) and performance benchmarks.

## Installation
Add project references to:
- `AdoLite.Core`
- `AdoLite.SqlServer`, `AdoLite.Postgres`, `AdoLite.MySql` as needed
- `AdoLite.Extension` for DI helpers

## Quick Start (DI)
```csharp
using AdoLite.Core.Base;
using AdoLite.Core.Enums;
using AdoLite.Extension;
using AdoLite.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddAdoLiteDataContexts(new DatabaseSettings
{
    Connections =
    {
        ["Default"] = new DatabaseConfig
        {
            Provider = DatabaseProvider.SqlServer,
            ConnectionString = "Server=.;Database=MyDb;User Id=sa;Password=...;Pooling=true;"
        }
    }
});

using var sp = services.BuildServiceProvider();
var factory = sp.GetRequiredService<IDataContextFactory>();
using var ctx = factory.Create("Default");

var user = ctx.GetSingleRecord<User>("SELECT TOP 1 Id, Name FROM Users");
```

## Multi-DB Example
```csharp
settings.Connections["SqlPrimary"] = new DatabaseConfig { Provider = DatabaseProvider.SqlServer, ConnectionString = "..." };
settings.Connections["Postgres"]   = new DatabaseConfig { Provider = DatabaseProvider.PostgreSQL, ConnectionString = "..." };
settings.Connections["MySql"]      = new DatabaseConfig { Provider = DatabaseProvider.MySQL, ConnectionString = "..." };

var ctxSql  = factory.Create("SqlPrimary");
var ctxPg   = factory.Create("Postgres");
var ctxMy   = factory.Create("MySql");
```

## Core Usage Examples
- **Get DataTable:** `ctx.GetDataTable("SELECT * FROM Customers WHERE Country = @c", new() { { "@c", "USA" } });`
- **Get Single Record:** `ctx.GetSingleRecord<User>("SELECT TOP 1 Id, Name FROM Users");`
- **Scalar:** `ctx.GetSingleValue<int>("SELECT COUNT(*) FROM Orders");`
- **Dictionary:** `ctx.GetDictionary<int,string>("SELECT Id, Name FROM Products WHERE Discontinued = 0");`
- **Paged:** `ctx.GetPagedDataTable("SELECT * FROM Orders ORDER BY OrderID", null, 1, 20);`
- **Transactions:** Use `QueryPattern` with `SaveChanges` to execute multiple commands atomically.
- **Bulk Insert:** `ctx.BulkInsert("Products", productsList);` (per provider implementations).
- **Stored Procedures:** `ctx.GetDataTable("EXEC CustOrderHist @CustomerID", new() { { "@CustomerID", "ALFKI" } });`

## Logging
Pass an `ILogger<DataQuery>` via DI; operations log SQL (trimmed), parameters, duration, and errors.

## Samples
- `AdoLite.Sample.SqlServer`, `AdoLite.Sample.Postgres`, `AdoLite.Sample.MySql`: basic usage per provider.
- `AdoLite.Sample.MultiDb`: simultaneous use of SQL Server, Postgres, MySQL.
- `AdoLite.Sample.Northwind`: real-world flow on the NORTHWND database (list data, create order, stored proc).

## Performance Benchmarks
`AdoLite.Tests/Performance/PerformanceBenchmarks.cs` provides basic latency checks (GetSingleValue/GetDataTable) for each provider. Thresholds are simple sanity checks; adjust to your environment for stricter expectations.

## Benefits
- Single, consistent API across providers.
- Easy DI integration and multi-DB support.
- Connection pooling-friendly (per-call open/close).
- Transaction and bulk helpers reduce boilerplate.
- Optional logging for observability.

## Limitations / Considerations
- Integration tests and samples require real databases; defaults are provided but should be replaced with secure connection strings.
- Raw SQL is used; ensure parameters are used for untrusted input. Typed parameters are recommended for complex types/TVPs.
- Nullability warnings remain; tighten annotations/guards for production use.
- Bulk implementations are simple row-by-row inserts (except SQL Server TVP support); for very large loads, consider provider-specific bulk APIs.

## Comparison to Other Tools
- **Dapper**: Dapper focuses on micro-ORM mapping; AdoLite focuses on provider-agnostic helpers (queries, transactions, bulk) with multi-DB DI support and logging hooks. You can still use Dapper on top of AdoLite’s connections if you prefer richer mapping.
- **Entity Framework Core**: EF Core provides full ORM (change tracking, LINQ-to-SQL, migrations). AdoLite is lighter-weight, SQL-centric, with explicit control over commands and parameters; no tracking or LINQ translation.
- **Raw ADO.NET**: AdoLite reduces boilerplate (opening/closing connections, mapping rows, transactions, bulk helpers) while keeping low-level control (SQL text, parameters).
```
