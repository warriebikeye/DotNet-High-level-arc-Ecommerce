namespace AuthServicex.Data;

// AuthService/Data/DbContext.cs
using Microsoft.Data.SqlClient;
using System.Data;

public class DbContext(IConfiguration config)
{
    private readonly string? _connectionString = config.GetConnectionString("AuthDb");

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
