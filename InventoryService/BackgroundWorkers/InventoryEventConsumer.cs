namespace InventoryServicex.BackgroundWorkers;

// InventoryService/BackgroundWorkers/InventoryEventConsumer.cs
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;


public class InventoryEventConsumer : BackgroundService
{
    private readonly ILogger<InventoryEventConsumer> _logger;
    private readonly IConsumer<string, string> _consumer;
    private readonly string _topic = "inventory-events";

    public InventoryEventConsumer(IConfiguration config, ILogger<InventoryEventConsumer> logger)
    {
        _logger = logger;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            GroupId = "inventory-service-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(stoppingToken);
                var inventoryData = result.Message.Value;

                _logger.LogInformation("Received inventory event: {InventoryData}", inventoryData);

                // Simulate background processing, e.g., updating database or performing business logic.
                await Task.Delay(1000, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing inventory event");
            }
        }
    }

    public override void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
        base.Dispose();
    }
}

