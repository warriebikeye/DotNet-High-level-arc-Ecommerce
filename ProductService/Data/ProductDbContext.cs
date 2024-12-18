namespace ProductServicex.Data;

// ProductService/Data/ProductDbContext.cs
using MongoDB.Driver;
using ProductServicex.models;

public class ProductDbContext
{
    private readonly IMongoDatabase _database;

    public ProductDbContext(IConfiguration config)
    {
        var client = new MongoClient(config.GetConnectionString("MongoDb"));
        _database = client.GetDatabase("ProductDb");
    }

    public IMongoCollection<Product> Products => _database.GetCollection<Product>("Products");
}

