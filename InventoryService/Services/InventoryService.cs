namespace InventoryServicex.Services;

using StackExchange.Redis;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;
using System.Text.Json;
using System.Threading.Tasks;
using global::InventoryServicex.models;


public class InventoryService
{
    private readonly ILogger<InventoryService> _logger;
    private readonly IDatabase _database;
    private readonly IProducer<string, string> _kafkaProducer;
    private readonly string _topic = "inventory-events";

    public InventoryService(ILogger<InventoryService> logger, IConnectionMultiplexer redis, IProducer<string, string> kafkaProducer)
    {
        _logger = logger;
        _database = redis.GetDatabase();
        _kafkaProducer = kafkaProducer;
    }

    public async Task<Inventory> GetInventoryAsync(string productId)
    {
        var stock = await _database.StringGetAsync(productId);
        return new Inventory
        {
            ProductId = productId,
            Stock = stock.HasValue ? (int)stock : 0
        };
    }

    public async Task UpdateInventoryAsync(string productId, int quantity)
    {
        // Update stock in Redis
        await _database.StringSetAsync(productId, quantity);

        // Log the update
        _logger.LogInformation("Inventory for product {ProductId} updated to {Quantity}", productId, quantity);

        // Publish to Kafka for further processing (e.g., updating databases)
        var inventoryJson = JsonSerializer.Serialize(new Inventory { ProductId = productId, Stock = quantity });
        await _kafkaProducer.ProduceAsync(_topic, new Message<string, string> { Key = productId, Value = inventoryJson });

        _logger.LogInformation("Inventory event published to Kafka for product {ProductId}", productId);
    }
}
