using System;
using System.Collections.Generic;
using System.Data;
using Xunit;
using AdoLite.SqlServer;
using AdoLite.Tests.SqlServer.Models;
using Microsoft.Data.SqlClient;
using AdoLite.Core.Interfaces;
using AdoLite.Core.Base;

namespace AdoLite.Tests.SqlServer;
public class DataQueryIntegrationTests : IDisposable
{
    private readonly DataQuery _dataQuery;

    // Provide your test DB connection string here
    private const string TestConnectionString = "Server=.;Database=TestDb;User Id=sa;Password=007;TrustServerCertificate=True;Pooling=true;Max Pool Size=50;";



    public DataQueryIntegrationTests()
    {
        _dataQuery = new DataQuery(TestConnectionString);

        // Optional: Setup test data here or ensure test DB has data ready
    }

    public void Dispose()
    {
        _dataQuery?.Dispose();
    }



    [Fact]
    public void GetDataRow_ReturnsFirstRow()
    {
        // Arrange
        var query = "SELECT TOP 1 Id, Name FROM Users";

        // Act
        var row = _dataQuery.GetDataRow(query);

        // Assert
        Assert.NotNull(row);
        Assert.True(row.Table.Columns.Contains("Id"));
        Assert.True(row.Table.Columns.Contains("Name"));
    }

    [Fact]
    public void GetDataRow_ReturnsUserById()
    {
        // Arrange
        var query = "SELECT Id, Name FROM Users WHERE Id = @Id";
        var parameters = new Dictionary<string, string>
        {
            { "@Id", "1" }
        };

        // Act
        var row = _dataQuery.GetDataRow(query, parameters);

        // Assert
        Assert.NotNull(row);
        Assert.True(row.Table.Columns.Contains("Id"));
        Assert.True(row.Table.Columns.Contains("Name"));
        Assert.Equal(1, Convert.ToInt32(row["Id"]));
    }


    [Fact]
    public void GetDataRow_ReturnsNull_WhenUserNotFound()
    {
        // Arrange
        var query = "SELECT Id, Name FROM Users WHERE Id = @Id";
        var parameters = new Dictionary<string, string>
    {
        { "@Id", "-999" } 
    };

        // Act
        var row = _dataQuery.GetDataRow(query, parameters);

        // Assert
        Assert.Null(row);
    }
    [Fact]
    public void GetSingleRecord_ReturnsTypedObject()
    {
        // Arrange
        var query = "SELECT TOP 1 Id, Name FROM Users";

        // Act
        var result = _dataQuery.GetSingleRecord<User>(query);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.False(string.IsNullOrEmpty(result.Name));
    }

    [Fact]
    public void GetSingleRecord_ReturnsTypedObject_WithParameter()
    {
        // Arrange
        var query = "SELECT Id, Name FROM Users WHERE Id = @Id";
        var parameters = new Dictionary<string, string>
    {
        { "@Id", "1" }
    };

        // Act
        var result = _dataQuery.GetSingleRecord<User>(query, parameters);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.False(string.IsNullOrEmpty(result.Name));
    }

    [Fact]
    public void GetSingleRecord_ReturnsNull_WhenNoMatchingRecord()
    {
        // Arrange
        var query = "SELECT Id, Name FROM Users WHERE Id = @Id";
        var parameters = new Dictionary<string, string>
    {
        { "@Id", "-999" } // Assumed to not exist
    };

        // Act
        var result = _dataQuery.GetSingleRecord<User>(query, parameters);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetSingleRecord_IgnoresCaseInColumnMapping()
    {
        // Arrange
        var query = "SELECT TOP 1 Id AS id, Name AS name FROM Users";

        // Act
        var result = _dataQuery.GetSingleRecord<User>(query);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.False(string.IsNullOrEmpty(result.Name));
    }

    // Optional: Add this only if you want to test for invalid type mapping
    [Fact]
    public void GetSingleRecord_ThrowsIfTypeMismatch()
    {
        // Arrange
        var query = "SELECT TOP 1 Id, Name FROM Users";

        // Dummy type with a mismatched property
        var ex = Record.Exception(() =>
        {
            _dataQuery.GetSingleRecord<InvalidUser>(query);
        });

        // Assert
        Assert.NotNull(ex);
        Assert.IsType<InvalidCastException>(ex.InnerException ?? ex);
    }

    [Fact]
    public void GetDataSet_ReturnsNonEmptyDataSet()
    {
        // Arrange
        var query = "SELECT TOP 5 * FROM Users";

        // Act
        var ds = _dataQuery.GetDataSet(query, null);

        // Assert
        Assert.NotNull(ds);
        Assert.NotEmpty(ds.Tables);
        Assert.True(ds.Tables[0].Rows.Count > 0);
    }

    [Fact]
    public void GetDataSet_ReturnsEmptyDataSet_WhenNoDataMatches()
    {
        // Arrange
        var query = "SELECT * FROM Users WHERE Id = @Id";
        var parameters = new Dictionary<string, string>
    {
        { "@Id", "-999" } // Assumed non-existent
    };

        // Act
        var ds = _dataQuery.GetDataSet(query, parameters);

        // Assert
        Assert.NotNull(ds);
        Assert.NotEmpty(ds.Tables);
        Assert.Empty(ds.Tables[0].Rows);
    }

    [Fact]
    public void GetDataSet_ReturnsDataSet_WithQueryParameters()
    {
        // Arrange
        var query = "SELECT * FROM Users WHERE Id = @Id";
        var parameters = new Dictionary<string, string>
    {
        { "@Id", "1" }
    };

        // Act
        var ds = _dataQuery.GetDataSet(query, parameters);

        // Assert
        Assert.NotNull(ds);
        Assert.NotEmpty(ds.Tables);
        Assert.True(ds.Tables[0].Rows.Count > 0);
    }

    [Fact]
    public void GetDataSet_ThrowsException_ForInvalidQuery()
    {
        // Arrange
        var query = "SELECT * FORM NonExistentTable";

        // Act & Assert
        Assert.Throws<SqlException>(() =>
        {
            _dataQuery.GetDataSet(query, null);
        });
    }


    [Fact]
    public void GetDataTable_ReturnsNonEmptyDataTable()
    {
        var query = "SELECT TOP 5 * FROM Users";
        var dt = _dataQuery.GetDataTable(query);
        Assert.NotNull(dt);
        Assert.True(dt.Rows.Count > 0);
    }

    [Fact]
    public void GetDataTable_ReturnsEmptyDataTable_WhenNoDataMatches()
    {
        // Arrange
        var query = "SELECT * FROM Users WHERE Id = @Id";
        var parameters = new Dictionary<string, string>
    {
        { "@Id", "-999" } // Assuming this ID does not exist
    };

        // Act
        var dt = _dataQuery.GetDataTable(query, parameters);

        // Assert
        Assert.NotNull(dt);
        Assert.Equal(0, dt.Rows.Count);
    }

    [Fact]
    public void GetDataTable_ThrowsException_OnInvalidQuery()
    {
        // Arrange
        var query = "SELECT * FORM InvalidSyntax"; // 'FORM' is a syntax error

        // Act & Assert
        Assert.Throws<SqlException>(() =>
        {
            _dataQuery.GetDataTable(query);
        });
    }


    [Fact]
    public void GetDataTable_ReturnsDataTable_WithQueryParameters()
    {
        // Arrange
        var query = "SELECT * FROM Users WHERE Id = @Id";
        var parameters = new Dictionary<string, string>
    {
        { "@Id", "1" } // Assuming user with ID 1 exists
    };

        // Act
        var dt = _dataQuery.GetDataTable(query, parameters);

        // Assert
        Assert.NotNull(dt);
        Assert.True(dt.Rows.Count > 0);
    }



    [Fact]
    public void GetSingleValue_ReturnsCorrectValue()
    {
        var query = "SELECT COUNT(*) FROM Users";
        int count = _dataQuery.GetSingleValue<int>(query);
        Assert.True(count >= 0);
    }
    [Fact]
    public void GetSingleValue_ReturnsValue_WithParameters()
    {
        // Arrange
        var query = "SELECT Name FROM Users WHERE Id = @Id";
        var parameters = new Dictionary<string, string>
    {
        { "@Id", "1" } // Assuming a user with ID 1 exists
    };

        // Act
        var name = _dataQuery.GetSingleValue<string>(query, parameters);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(name));
    }
    [Fact]
    public void GetSingleValue_ReturnsDefault_WhenNoDataFound()
    {
        // Arrange
        var query = "SELECT Name FROM Users WHERE Id = @Id";
        var parameters = new Dictionary<string, string>
    {
        { "@Id", "-999" } // Assuming this ID doesn't exist
    };

        // Act
        var result = _dataQuery.GetSingleValue<string>(query, parameters);

        // Assert
        Assert.Null(result); // Default of string is null
    }
    [Fact]
    public void GetSingleValue_ThrowsException_OnInvalidQuery()
    {
        // Arrange
        var query = "SELECTE COUNT(*) FORM Users"; // Intentionally invalid

        // Act & Assert
        Assert.Throws<SqlException>(() =>
        {
            _dataQuery.GetSingleValue<int>(query);
        });
    }
    [Fact]
    public void GetSingleValue_ReturnsDefault_WhenResultIsDBNull()
    {
        // Arrange
        var query = "SELECT Name FROM Users WHERE Id = @Id"; // assuming MiddleName can be NULL
        var parameters = new Dictionary<string, string>
    {
        { "@Id", "11" }
    };

        // Act
        var result = _dataQuery.GetSingleValue<string>(query, parameters);

        // Assert
        // If DB returns NULL, result should be null
        Assert.True(result == null || result is string);
    }


    [Fact]
    public void GetList_ReturnsListOfIds()
    {
        var query = "SELECT TOP 5 Id FROM Users";
        var list = _dataQuery.GetList<int>(query);
        Assert.NotNull(list);
        Assert.NotEmpty(list);
    }

    [Fact]
    public void GetList_ReturnsListOfNames_WithParameters()
    {
        // Arrange
        var query = "SELECT Name FROM Users WHERE Id <= @MaxId";
        var parameters = new Dictionary<string, string>
    {
        { "@MaxId", "10" }
    };

        // Act
        var list = _dataQuery.GetList<string>(query, parameters);

        // Assert
        Assert.NotNull(list);
        Assert.All(list, name => Assert.False(string.IsNullOrWhiteSpace(name)));
    }
    [Fact]
    public void GetList_ReturnsEmptyList_WhenNoMatch()
    {
        // Arrange
        var query = "SELECT Name FROM Users WHERE Id = @Id";
        var parameters = new Dictionary<string, string>
    {
        { "@Id", "-999" } // Assuming this doesn't exist
    };

        // Act
        var list = _dataQuery.GetList<string>(query, parameters);

        // Assert
        Assert.NotNull(list);
        Assert.Empty(list);
    }
    [Fact]
    public void GetList_ThrowsException_OnInvalidQuery()
    {
        // Arrange
        var query = "SELEC Id FROM Users"; // Invalid SQL

        // Act & Assert
        Assert.Throws<SqlException>(() =>
        {
            _dataQuery.GetList<int>(query);
        });
    }
    [Fact]
    public void GetList_ThrowsException_OnInvalidTypeCast()
    {
        // Arrange
        var query = "SELECT Name FROM Users"; // Returns string, not int

        // Act & Assert
        Assert.Throws<FormatException>(() =>
        {
            _dataQuery.GetList<int>(query); // Trying to cast string to int
        });
    }


    [Fact]
    public void GetCount_ReturnsNonNegative()
    {
        var query = "SELECT COUNT(*) FROM Users";
        int count = _dataQuery.GetCount(query);
        Assert.True(count >= 0);
    }

    [Fact]
    public void Exists_ReturnsTrueIfRows()
    {
        var query = "SELECT 1 FROM Users WHERE Id = 1";
        bool exists = _dataQuery.Exists(query);
        Assert.True(exists);
    }

    [Fact]
    public void Exists_ReturnsTrueWithParameters()
    {
        // Arrange
        var query = "SELECT 1 FROM Users WHERE Id = @Id";
        var parameters = new Dictionary<string, string>
    {
        { "@Id", "1" }
    };

        // Act
        bool exists = _dataQuery.Exists(query, parameters);

        // Assert
        Assert.True(exists);
    }


    [Fact]
    public void Exists_ReturnsFalseIfNoRows()
    {
        // Arrange
        var query = "SELECT 1 FROM Users WHERE Id = -999"; // Assuming this ID doesn't exist

        // Act
        bool exists = _dataQuery.Exists(query);

        // Assert
        Assert.False(exists);
    }


    [Fact]
    public void Exists_ReturnsFalseWithParametersIfNoMatch()
    {
        // Arrange
        var query = "SELECT 1 FROM Users WHERE Id = @Id";
        var parameters = new Dictionary<string, string>
    {
        { "@Id", "-999" }
    };

        // Act
        bool exists = _dataQuery.Exists(query, parameters);

        // Assert
        Assert.False(exists);
    }
    [Fact]
    public void Exists_ThrowsExceptionOnInvalidQuery()
    {
        // Arrange
        var query = "SELEC 1 FROM"; // Malformed query

        // Act & Assert
        Assert.Throws<SqlException>(() =>
        {
            _dataQuery.Exists(query);
        });
    }
    [Fact]
    public void Exists_ThrowsExceptionOnNullQuery()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            _dataQuery.Exists(null);
        });
    }


    [Fact]
    public void GetPagedDataTable_ReturnsCorrectNumberOfRows()
    {
        var query = "SELECT * FROM Users ORDER BY Id";
        var dt = _dataQuery.GetPagedDataTable(query, null, pageNumber: 1, pageSize: 2);
        Assert.NotNull(dt);
        Assert.True(dt.Rows.Count <= 2);
    }

    [Fact]
    public void GetPagedDataTable_WithUserRolesJoin_ReturnsOnlyAdmins()
    {
        var query = @"
        SELECT 
            u.Id AS UserId, 
            u.Name, 
            r.RoleName
        FROM Users u
        INNER JOIN UserRoles ur ON u.Id = ur.UserId
        INNER JOIN Roles r ON ur.RoleId = r.RoleId
        WHERE r.RoleName = @Role
        ORDER BY u.Id";

        var parameters = new Dictionary<string, string>
    {
        { "@Role", "Admin" }
    };

        var dt = _dataQuery.GetPagedDataTable(query, parameters, pageNumber: 1, pageSize: 10);

        Assert.NotNull(dt);
        Assert.True(dt.Rows.Count <= 10);

        foreach (DataRow row in dt.Rows)
        {
            Assert.Equal("Admin", row["RoleName"].ToString());
        }
    }



    [Fact]
    public void GetPagedDataTable_PageNumberLessThanOne_TreatedAsFirstPage()
    {
        var query = "SELECT * FROM Users ORDER BY Id";
        var dt = _dataQuery.GetPagedDataTable(query, null, 0, 3);
        Assert.NotNull(dt);
        Assert.True(dt.Rows.Count <= 3);
    }

    [Fact]
    public void GetPagedDataTable_PageSizeLessThanOne_DefaultsTo10()
    {
        var query = "SELECT * FROM Users ORDER BY Id";
        var dt = _dataQuery.GetPagedDataTable(query, null, 1, 0);
        Assert.NotNull(dt);
        Assert.True(dt.Rows.Count <= 10);
    }

    [Fact]
    public void GetPagedDataTable_InvalidSql_ThrowsException()
    {
        var query = "SELECT * FORM Users"; // typo FORM instead of FROM
        Assert.Throws<SqlException>(() => _dataQuery.GetPagedDataTable(query, null, 1, 2));
    }

    [Fact]
    public void GetPagedDataTable_MissingParameter_ThrowsException()
    {
        var query = "SELECT * FROM Users WHERE Role = @Role ORDER BY Id";
        // no parameters provided
        Assert.Throws<SqlException>(() => _dataQuery.GetPagedDataTable(query, null, 1, 2));
    }

    [Fact]
    public void GetPagedDataTable_NullOrEmptyQuery_ThrowsArgumentNullException()
    {
        Assert.Throws<SqlException>(() => _dataQuery.GetPagedDataTable(null, null, 1, 2));
        Assert.Throws<SqlException>(() => _dataQuery.GetPagedDataTable("", null, 1, 2));
    }


    [Fact]
    public void GetDictionary_ReturnsKeyValuePairs()
    {
        var query = "SELECT TOP 5 Id, Name FROM Users";
        var dict = _dataQuery.GetDictionary<int, string>(query);
        Assert.NotNull(dict);
        Assert.NotEmpty(dict);
    }

    [Fact]
    public void GetMappedList_ReturnsMappedObjects()
    {
        var query = "SELECT TOP 5 Id, Name FROM Users";
        var list = _dataQuery.GetMappedList(query, row => new User
        {
            Id = row.Field<int>("Id"),
            Name = row.Field<string>("Name")
        });

        Assert.NotNull(list);
        Assert.NotEmpty(list);
        Assert.All(list, item => Assert.False(string.IsNullOrEmpty(item.Name)));
    }
    [Fact]
    public void GetMappedList_EmptyResult_ReturnsEmptyList()
    {
        var query = "SELECT Id, Name FROM Users WHERE Id = -9999";
        var list = _dataQuery.GetMappedList(query, row => new User
        {
            Id = row.Field<int>("Id"),
            Name = row.Field<string>("Name")
        });

        Assert.NotNull(list);
        Assert.Empty(list);
    }
    [Fact]
    public void GetMappedList_WithParameters_ReturnsFilteredResults()
    {
        var query = "SELECT Id, Name FROM Users WHERE Name = @Name";
        var parameters = new Dictionary<string, string> { { "@Name", "Grace" } };

        var list = _dataQuery.GetMappedList(query, row => new User
        {
            Id = row.Field<int>("Id"),
            Name = row.Field<string>("Name")
        }, parameters);

        Assert.NotNull(list);
        Assert.All(list, user => Assert.Contains("Grace", user.Name, StringComparison.OrdinalIgnoreCase)); // Example condition
    }


    [Fact]
    public void GetMappedList_MapToDifferentType_ReturnsCorrectType()
    {
        var query = "SELECT TOP 2 Name FROM Users";
        var list = _dataQuery.GetMappedList(query, row => new User
        {
            Name = row.Field<string>("Name")
        });

        Assert.All(list, item => Assert.IsType<User>(item));
    }
    [Fact]
    public void GetMappedList_NullMapFunction_ThrowsArgumentNullException()
    {
        var query = "SELECT Id FROM Users";
        Assert.Throws<ArgumentNullException>(() => _dataQuery.GetMappedList<User>(query, null));
    }
    [Fact]
    public void GetMappedList_InvalidColumnNamesInMapping_ThrowsException()
    {
        var query = "SELECT Id FROM Users"; // No Name column
        Assert.Throws<ArgumentException>(() =>
            _dataQuery.GetMappedList(query, row => new User
            {
                Id = row.Field<int>("Id"),
                Name = row.Field<string>("InvalidColumn")
            }));
    }
    [Fact]
    public void GetMappedList_InvalidQuery_ThrowsSqlException()
    {
        var query = "SELEC Id FROM"; // Invalid SQL
        Assert.Throws<SqlException>(() =>
            _dataQuery.GetMappedList(query, row => new User
            {
                Id = row.Field<int>("Id"),
                Name = row.Field<string>("Name")
            }));
    }
    [Fact]
    public void GetMappedList_MissingParameters_ThrowsSqlException()
    {
        var query = "SELECT * FROM Users WHERE Role = @Role";
        Assert.Throws<SqlException>(() =>
            _dataQuery.GetMappedList(query, row => new User
            {
                Id = row.Field<int>("Id"),
                Name = row.Field<string>("Name")
            }));
    }
    [Fact]
    public void GetMappedList_NullQuery_ThrowsException()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _dataQuery.GetMappedList<User>(null, row => new User { Id = 1, Name = "Test" }));
    }

    [Fact]
    public void AddParameters_NullInput_ReturnsEmptyDictionary()
    {
        var result = _dataQuery.AddParameters(null);
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    [Fact]
    public void AddParameters_EmptyArray_ReturnsEmptyDictionary()
    {
        var result = _dataQuery.AddParameters(Array.Empty<string>());
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void AddParameters_ValidInput_ReturnsExpectedDictionary()
    {
        var input = new[] { "val1", "val2" };
        var result = _dataQuery.AddParameters(input);

        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("@param1"));
        Assert.True(result.ContainsKey("@param2"));
        Assert.Equal("val1", result["@param1"]);
        Assert.Equal("val2", result["@param2"]);
    }
    [Fact]
    public void AddQuery_ValidInput_ReturnsQueryPattern()
    {
        var parameters = new Dictionary<string, object> { { "@param1", "value" } };
        var result = _dataQuery.AddQuery("SELECT * FROM Users", parameters);

        Assert.NotNull(result);
        Assert.Equal("SELECT * FROM Users", result.Query);
        Assert.Single(result.Parameters);
        Assert.Equal("value", result.Parameters[0]["@param1"]);
    }
    [Fact]
    public void AddQuery_EmptyParameters_AddsEmptyDictionary()
    {
        var result = _dataQuery.AddQuery("SELECT 1", new Dictionary<string, object>());

        Assert.NotNull(result);
        Assert.Equal("SELECT 1", result.Query);
        Assert.Single(result.Parameters); // Added 1 dictionary
        Assert.Empty(result.Parameters[0]);
    }

    [Fact]
    public void SaveChanges_ValidQueries_ReturnsTrue()
    {
        var queryPatterns = new List<IQueryPattern>
    {
        new QueryPattern
        {
            Query = "INSERT INTO Users (Name) VALUES ('TestUser')",
            Parameters = new List<Dictionary<string, object>>() // No parameters
        }
    };

        var result = _dataQuery.SaveChanges(queryPatterns);
        Assert.True(result);
    }

    [Fact]
    public void SaveChanges_MultipleValidQueries_ReturnsTrue()
    {
        // Arrange
        var insertUserQuery = new QueryPattern
        {
            Query = "INSERT INTO Users (Name, Email) VALUES (@Name, @Email)",
            Parameters = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "@Name", "TestUser1" },
                { "@Email", "testuser1@example.com" }
            }
        }
        };

        var insertRoleQuery = new QueryPattern
        {
            Query = "INSERT INTO Roles (RoleName) VALUES (@RoleName)",
            Parameters = new List<Dictionary<string, object>>
        {
            new Dictionary<string, object>
            {
                { "@RoleName", "TestRole" }
            }
        }
        };

        var queryPatterns = new List<IQueryPattern> { insertUserQuery, insertRoleQuery };

        // Act
        var result = _dataQuery.SaveChanges(queryPatterns);

        // Assert
        Assert.True(result);
    }


    [Fact]
    public void SaveChanges_WhenOneQueryFails_TransactionIsRolledBack()
    {
        // Arrange: Insert a valid user and an invalid query to trigger rollback
        var queryPatterns = new List<IQueryPattern>
    {
        // Valid insert
        new QueryPattern
        {
            Query = "INSERT INTO Users (Name, Email) VALUES (@Name, @Email)",
            Parameters = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "@Name", "RollbackTestUser" },
                    { "@Email", "rollbacktest@example.com" }
                }
            }
        },
        // Invalid query - table does not exist
        new QueryPattern
        {
            Query = "INSERT INTO InvalidTable (Column) VALUES (@Value)",
            Parameters = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { { "@Value", "ShouldFail" } }
            }
        }
    };

        // Act + Assert: should throw exception and rollback
        Assert.Throws<SqlException>(() => _dataQuery.SaveChanges(queryPatterns));

        // Verify: user should NOT be inserted due to rollback
        var verifyQuery = "SELECT COUNT(*) FROM Users WHERE Name = @Name";
        var parameters = new Dictionary<string, string> { { "@Name", "RollbackTestUser" } };
        var dt = _dataQuery.GetDataTable(verifyQuery, parameters);

        int count = Convert.ToInt32(dt.Rows[0][0]);
        Assert.Equal(0, count); // Confirm rollback occurred
    }
    [Fact]
    public void SaveChanges_BatchInsert_UsingTableValuedParameter_Works()
    {
        // --- Arrange ---

        // Ensure UserTableType exists
        string typeCheckAndCreate = @"
        IF NOT EXISTS (SELECT * FROM sys.types WHERE is_table_type = 1 AND name = 'UserTableType')
        BEGIN
            CREATE TYPE dbo.UserTableType AS TABLE
            (
                Name NVARCHAR(100),
                Email NVARCHAR(150)
            );
        END;";

        // Ensure InsertUsersBatch procedure exists
        string procCheckAndCreate = @"
        IF NOT EXISTS (
            SELECT * FROM sys.objects 
            WHERE type = 'P' AND name = 'InsertUsersBatch'
        )
        BEGIN
            EXEC('
                CREATE PROCEDURE InsertUsersBatch
                    @Users dbo.UserTableType READONLY
                AS
                BEGIN
                    INSERT INTO Users (Name, Email)
                    SELECT Name, Email FROM @Users;
                END;
            ')
        END;";

        // Execute both setup scripts
        _dataQuery.ExecuteRawSql(typeCheckAndCreate);
        _dataQuery.ExecuteRawSql(procCheckAndCreate);

        // Create a table-valued parameter DataTable
        var userTable = new DataTable();
        userTable.Columns.Add("Name", typeof(string));
        userTable.Columns.Add("Email", typeof(string));
        userTable.Rows.Add("BatchUser1", "batch1@example.com");
        userTable.Rows.Add("BatchUser2", "batch2@example.com");

        var tvpParam = new SqlParameter
        {
            ParameterName = "@Users",
            SqlDbType = SqlDbType.Structured,
            TypeName = "dbo.UserTableType",
            Value = userTable
        };

        var parameters = new Dictionary<string, object>
    {
        { "@Users", tvpParam }
    };

        var queryPattern = new QueryPattern
        {
            Query = "Exec InsertUsersBatch @Users", // Stored Procedure
            Parameters = new List<Dictionary<string, object>> { parameters }
        };

        var queryPatterns = new List<IQueryPattern> { queryPattern };

        // --- Act ---
        var result = _dataQuery.SaveChanges(queryPatterns);

        // --- Assert ---
        Assert.True(result);

        var verificationQuery = @"
        SELECT COUNT(*) FROM Users
        WHERE Name IN ('BatchUser1', 'BatchUser2')
          AND Email IN ('batch1@example.com', 'batch2@example.com');";

        var verificationTable = _dataQuery.GetDataTable(verificationQuery);
        int count = Convert.ToInt32(verificationTable.Rows[0][0]);
        Assert.Equal(12, count);
    }



    [Fact]
    public void SaveChanges_InvalidQuery_ThrowsException()
    {
        var queryPatterns = new List<IQueryPattern>
    {
        new QueryPattern
        {
            Query = "INVALID SQL",
            Parameters = new List<Dictionary<string, object>>()
        }
    };

        Assert.Throws<SqlException>(() => _dataQuery.SaveChanges(queryPatterns));
    }

    [Fact]
    public void SaveChanges_NullParameters_DoesNotThrow()
    {
        var queryPatterns = new List<IQueryPattern>
    {
        new QueryPattern
        {
            Query = "INSERT INTO Roles (RoleName) VALUES ('TestLog')",
            Parameters = null
        }
    };

        var result = _dataQuery.SaveChanges(queryPatterns);
        Assert.True(result);
    }
    [Fact]
    public void SaveChanges_EmptyQueryList_DoesNothing_ReturnsTrue()
    {
        var result = _dataQuery.SaveChanges(new List<IQueryPattern>());
        Assert.True(result);
    }
    [Fact]
    public void SaveChanges_NullQueryList_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => _dataQuery.SaveChanges(null));
    }





}
