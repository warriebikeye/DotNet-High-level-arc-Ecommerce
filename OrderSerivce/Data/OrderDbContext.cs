namespace OrderSerivce.Data;

// OrderService/Data/OrderDbContext.cs
using Microsoft.Data.SqlClient;
using System.Data;
public class OrderDbContext
{
    private readonly IConfiguration _configuration;
    public OrderDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IDbConnection CreateConnection() => new SqlConnection(_configuration.GetConnectionString("OrderDb"));
}
