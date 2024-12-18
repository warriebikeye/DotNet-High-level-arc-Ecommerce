namespace ProductServicex.Services;

// ProductService/Services/ProductService.cs
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using global::ProductServicex.Data;
using global::ProductServicex.models;

public class ProductService
{
    private readonly ProductDbContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(ProductDbContext context, ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        _logger.LogInformation("Fetching all products from the database.");
        return await _context.Products.Find(_ => true).ToListAsync();
    }

    public async Task<Product> GetProductByIdAsync(string id)
    {
        _logger.LogInformation("Fetching product with ID: {Id}", id);
        return await _context.Products.Find(p => p.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreateProductAsync(Product product)
    {
        _logger.LogInformation("Adding new product: {Name}", product.Name);
        await _context.Products.InsertOneAsync(product);
    }

    public async Task UpdateProductAsync(string id, Product updatedProduct)
    {
        _logger.LogInformation("Updating product with ID: {Id}", id);
        await _context.Products.ReplaceOneAsync(p => p.Id == id, updatedProduct);
    }

    public async Task DeleteProductAsync(string id)
    {
        _logger.LogInformation("Deleting product with ID: {Id}", id);
        await _context.Products.DeleteOneAsync(p => p.Id == id);
    }
}

