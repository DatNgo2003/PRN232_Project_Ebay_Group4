using Backend.DTOs.Responses;
using Backend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CouponController : ControllerBase
    {
        private readonly ICouponRepository _couponRepository;

        public CouponController(ICouponRepository couponRepository)
        {
            _couponRepository = couponRepository;
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableCoupons([FromQuery] int productId)
        {
            if (productId <= 0)
            {
                return BadRequest(new { message = "productId không hợp lệ." });
            }

            try
            {
                var coupons = await _couponRepository.GetAvailableForProductAsync(productId);

                var dtos = coupons.Select(c => new CouponDto
                {
                    Id = c.Id,
                    Code = c.Code ?? "",
                    DiscountPercent = c.DiscountPercent ?? 0,
                    EndDate = c.EndDate,
                    MaxUsage = c.MaxUsage,
                    UsedCount = c.UsedCount
                });

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}