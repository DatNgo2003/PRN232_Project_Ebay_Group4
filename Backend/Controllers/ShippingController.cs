using Backend.Services;
using Backend.Services.Shipping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/shipping")]
public class ShippingController : ControllerBase
{
    private readonly IShippingService _shippingService;

    public ShippingController(IShippingService shippingService)
    {
        _shippingService = shippingService;
    }

    /// <summary>
    /// Get all available shipping carriers.
    /// GET /api/shipping/carriers
    /// </summary>
    [HttpGet("carriers")]
    public IActionResult GetCarriers()
    {
        var carriers = _shippingService.GetAvailableCarriers();
        return Ok(carriers);
    }

    /// <summary>
    /// Estimate shipping fee for a specific carrier.
    /// POST /api/shipping/estimate-fee
    /// </summary>
    [HttpPost("estimate-fee")]
    [Authorize]
    public IActionResult EstimateFee([FromBody] EstimateFeeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CarrierKey))
            return BadRequest(new { message = "CarrierKey is required." });

        if (request.OrderTotal < 0)
            return BadRequest(new { message = "OrderTotal must be non-negative." });

        try
        {
            var destination = new Backend.Models.Address
            {
                City = request.City,
                Country = request.Country
            };

            var fee = _shippingService.EstimateFee(destination, request.OrderTotal, request.CarrierKey);
            return Ok(new
            {
                carrierKey = request.CarrierKey,
                fee,
                orderTotal = request.OrderTotal
            });
        }
        catch (NotSupportedException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class EstimateFeeRequest
{
    public string CarrierKey { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Country { get; set; }
    public decimal OrderTotal { get; set; }
}
