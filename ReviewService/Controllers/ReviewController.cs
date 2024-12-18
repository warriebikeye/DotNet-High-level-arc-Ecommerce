namespace ReviewServicex.Controllers;

using global::ReviewServicex.models;

// ReviewService/Controllers/ReviewController.cs
using Microsoft.AspNetCore.Mvc;
using ReviewServicex.Services;


[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly ReviewService _reviewService;

    public ReviewController(ReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    public async Task<IActionResult> AddReview([FromBody] Review review)
    {
        var newReview = await _reviewService.AddReviewAsync(review);
        return CreatedAtAction(nameof(GetReviewsByProductId), new { productId = newReview.ProductId }, newReview);
    }

    [HttpGet("{productId}")]
    public async Task<IActionResult> GetReviewsByProductId(Guid productId)
    {
        var reviews = await _reviewService.GetReviewsByProductIdAsync(productId);
        return Ok(reviews);
    }
}

