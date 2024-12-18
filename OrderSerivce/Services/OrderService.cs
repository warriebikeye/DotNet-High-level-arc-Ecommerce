namespace OrderSerivce.Services;

using Dapper;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;
using System.Text.Json;
using OrderSerivce.Data;
using OrderSerivce.models;

public class OrderService
{
    private readonly OrderDbContext _dbContext;
    private readonly ILogger<OrderService> _logger;
    private readonly IProducer<string, string> _kafkaProducer;
    private readonly string _topic = "order-events";

    public OrderService(OrderDbContext dbContext, ILogger<OrderService> logger, IProducer<string, string> kafkaProducer)
    {
        _dbContext = dbContext;
        _logger = logger;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        order.Id = Guid.NewGuid();
        order.CreatedAt = DateTime.UtcNow;

        using var connection = _dbContext.CreateConnection();
        var query = "INSERT INTO Orders (Id, UserId, TotalAmount, Status, CreatedAt) VALUES (@Id, @UserId, @TotalAmount, @Status, @CreatedAt)";
        await connection.ExecuteAsync(query, order);

        _logger.LogInformation("Order created with ID: {Id}", order.Id);

        // Publish to Kafka
        var orderJson = JsonSerializer.Serialize(order);
        await _kafkaProducer.ProduceAsync(_topic, new Message<string, string> { Key = order.Id.ToString(), Value = orderJson });

        _logger.LogInformation("Order event published to Kafka for order ID: {Id}", order.Id);

        return order;
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        using var connection = _dbContext.CreateConnection();
        var query = "SELECT * FROM Orders";
        return await connection.QueryAsync<Order>(query);
    }

    public async Task<Order> GetOrderByIdAsync(Guid id)
    {
        using var connection = _dbContext.CreateConnection();
        var query = "SELECT * FROM Orders WHERE Id = @Id";
        return await connection.QuerySingleOrDefaultAsync<Order>(query, new { Id = id });
    }
}
