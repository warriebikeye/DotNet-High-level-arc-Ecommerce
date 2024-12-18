namespace InventoryServicex.Controllers;

// InventoryService/Controllers/InventoryController.cs
using Microsoft.AspNetCore.Mvc;
using InventoryServicex.Services;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly InventoryService _inventoryService;

    public InventoryController(InventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("{productId}")]
    public async Task<IActionResult> GetInventory(string productId)
    {
        var inventory = await _inventoryService.GetInventoryAsync(productId);
        return Ok(inventory);
    }

    [HttpPost("{productId}")]
    public async Task<IActionResult> UpdateInventory(string productId, [FromBody] int quantity)
    {
        await _inventoryService.UpdateInventoryAsync(productId, quantity);
        return NoContent();
    }
}

