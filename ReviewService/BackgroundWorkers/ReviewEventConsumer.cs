namespace ReviewServicex.BackgroundWorkers;

// ReviewService/BackgroundWorkers/ReviewEventConsumer.cs
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;


public class ReviewEventConsumer : BackgroundService
{
    private readonly ILogger<ReviewEventConsumer> _logger;
    private readonly IConsumer<string, string> _consumer;
    private readonly string _topic = "review-events";

    public ReviewEventConsumer(IConfiguration config, ILogger<ReviewEventConsumer> logger)
    {
        _logger = logger;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            GroupId = "review-service-group",
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
                var reviewData = result.Message.Value;

                _logger.LogInformation("Received review event: {ReviewData}", reviewData);

                // Simulate background processing
                await Task.Delay(1000, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing review event");
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

