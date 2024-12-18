namespace ReviewServicex.Services;

// ReviewService/Services/ReviewService.cs
using Cassandra;
using Microsoft.Extensions.Logging;
using ReviewServicex.models;
using Confluent.Kafka;
using System.Text.Json;
using System.Threading.Tasks;


public class ReviewService
{
    private readonly ILogger<ReviewService> _logger;
    private readonly ISession _session;
    private readonly IProducer<string, string> _kafkaProducer;
    private readonly string _topic = "review-events";

    public ReviewService(ILogger<ReviewService> logger, ISession session, IProducer<string, string> kafkaProducer)
    {
        _logger = logger;
        _session = session;  // Cassandra session
        _kafkaProducer = kafkaProducer;
    }

    public async Task<Review> AddReviewAsync(Review review)
    {
        review.Id = Guid.NewGuid();
        review.CreatedAt = DateTime.UtcNow;

        // Insert into Cassandra
        var query = "INSERT INTO reviews (id, product_id, user_id, comment, rating, created_at) VALUES (?, ?, ?, ?, ?, ?)";
        var preparedStatement = await _session.PrepareAsync(query);
        var boundStatement = preparedStatement.Bind(review.Id, review.ProductId, review.UserId, review.Comment, review.Rating, review.CreatedAt);
        await _session.ExecuteAsync(boundStatement);

        _logger.LogInformation("Review added for product {ProductId} by user {UserId}", review.ProductId, review.UserId);

        // Publish to Kafka for further processing
        var reviewJson = JsonSerializer.Serialize(review);
        await _kafkaProducer.ProduceAsync(_topic, new Message<string, string> { Key = review.ProductId.ToString(), Value = reviewJson });

        _logger.LogInformation("Review event published to Kafka for product {ProductId}", review.ProductId);

        return review;
    }

    public async Task<IEnumerable<Review>> GetReviewsByProductIdAsync(Guid productId)
    {
        var query = "SELECT * FROM reviews WHERE product_id = ?";
        var preparedStatement = await _session.PrepareAsync(query);
        var boundStatement = preparedStatement.Bind(productId);
        var rows = await _session.ExecuteAsync(boundStatement);

        var reviews = rows.Select(row => new Review
        {
            Id = row.GetValue<Guid>("id"),
            ProductId = row.GetValue<Guid>("product_id"),
            UserId = row.GetValue<Guid>("user_id"),
            Comment = row.GetValue<string>("comment"),
            Rating = row.GetValue<int>("rating"),
            CreatedAt = row.GetValue<DateTime>("created_at")
        });

        return reviews;
    }
}

