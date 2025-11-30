using System.Data;
using AdoLite.Core.Base;
using AdoLite.Core.Enums;
using AdoLite.Core.Interfaces;
using AdoLite.Extension;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Sample that operates on the NORTHWND database: lists customers/products, creates an order with details, and calls a stored procedure.

const string DefaultConn = "Server=.;Database=NORTHWND;User Id=sa;Password=007;TrustServerCertificate=True;Pooling=true;Max Pool Size=50;";
var connString = Environment.GetEnvironmentVariable("ADOLITE_SQLSERVER_CONNECTION") ?? DefaultConn;

var services = new ServiceCollection();
services.AddLogging(b => b.AddSimpleConsole());
services.AddAdoLiteDataContexts(new DatabaseSettings
{
    Connections =
    {
        ["Northwind"] = new DatabaseConfig
        {
            Provider = DatabaseProvider.SqlServer,
            ConnectionString = connString
        }
    }
});

using var sp = services.BuildServiceProvider();
var factory = sp.GetRequiredService<IDataContextFactory>();
using var ctx = factory.Create("Northwind");

// 1) List top customers
var customers = ctx.GetMappedList(
    "SELECT TOP 5 CustomerID, CompanyName, Country FROM Customers ORDER BY CustomerID",
    row => new
    {
        Id = row.Field<string>("CustomerID"),
        Name = row.Field<string>("CompanyName"),
        Country = row.Field<string>("Country")
    });
Console.WriteLine("Top customers:");
foreach (var c in customers) Console.WriteLine($"{c.Id} - {c.Name} ({c.Country})");

// 2) List products by category
var products = ctx.GetMappedList(
    @"SELECT TOP 5 p.ProductID, p.ProductName, c.CategoryName, p.UnitPrice
      FROM Products p JOIN Categories c ON p.CategoryID = c.CategoryID
      ORDER BY p.ProductID",
    row => new
    {
        Id = row.Field<int>("ProductID"),
        Name = row.Field<string>("ProductName"),
        Category = row.Field<string>("CategoryName"),
        Price = row.Field<decimal>("UnitPrice")
    });
Console.WriteLine("\nSample products:");
foreach (var p in products) Console.WriteLine($"{p.Id}: {p.Name} ({p.Category}) - {p.Price:C}");

// 3) Create a new order with details (simplified)
var newOrderId = CreateOrder(ctx, customerId: "ALFKI", employeeId: 1, shipVia: 1, freight: 10m);
Console.WriteLine($"\nCreated OrderID: {newOrderId}");

AddOrderDetail(ctx, newOrderId, productId: 1, unitPrice: 18m, quantity: 2, discount: 0);
AddOrderDetail(ctx, newOrderId, productId: 2, unitPrice: 19m, quantity: 1, discount: 0);
Console.WriteLine("Added order details.");

// 4) Run a stored procedure (CustOrderHist)
var hist = ctx.GetDataTable("EXEC CustOrderHist @CustomerID", new() { { "@CustomerID", "ALFKI" } });
Console.WriteLine("\nCustOrderHist for ALFKI:");
foreach (DataRow row in hist.Rows)
    Console.WriteLine($"{row["ProductName"]}: {row["Total"]}");

int CreateOrder(IDataContext c, string customerId, int employeeId, int shipVia, decimal freight)
{
    var sql = @"INSERT INTO Orders (CustomerID, EmployeeID, OrderDate, RequiredDate, ShipVia, Freight)
                VALUES (@CustomerID, @EmployeeID, GETDATE(), DATEADD(day, 7, GETDATE()), @ShipVia, @Freight);
                SELECT SCOPE_IDENTITY();";

    var orderId = c.GetSingleValue<decimal>(sql, new()
    {
        { "@CustomerID", customerId },
        { "@EmployeeID", employeeId.ToString() },
        { "@ShipVia", shipVia.ToString() },
        { "@Freight", freight.ToString() }
    });
    return (int)orderId;
}

void AddOrderDetail(IDataContext c, int orderId, int productId, decimal unitPrice, short quantity, float discount)
{
    var qp = new QueryPattern
    {
        Query = @"INSERT INTO OrderDetails (OrderID, ProductID, UnitPrice, Quantity, Discount)
                  VALUES (@OrderID, @ProductID, @UnitPrice, @Quantity, @Discount)",
        Parameters = new List<Dictionary<string, object>>
        {
            new()
            {
                { "@OrderID", orderId },
                { "@ProductID", productId },
                { "@UnitPrice", unitPrice },
                { "@Quantity", quantity },
                { "@Discount", discount }
            }
        }
    };
    c.SaveChanges(new List<IQueryPattern> { qp });
}
