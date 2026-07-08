using Backend.DTOs.Dispute;
using Backend.Repositories;
using Backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisputesController : ControllerBase
    {
        private readonly IDisputeService _service;
        public DisputesController(IDisputeService service)
        {
            _service = service;
        }
        [HttpGet("buyer/{buyerId}")]
        public async Task<ActionResult<IEnumerable<DisputeListItemDto>>> GetBuyerDisputes(
             int buyerId)
        {
            if (buyerId <= 0)
            {
                return BadRequest("buyerId không hợp lệ");
            }

            var disputes = await _service.GetDisputesByBuyerAsync(buyerId);

            return Ok(disputes);
        }

        /// <summary>
        /// Seller – danh sách khiếu nại đã nhận
        /// Ví dụ: GET api/disputes/seller?sellerId=8
        /// </summary>
        [HttpGet("seller/{sellerId}")]
        public async Task<ActionResult<IEnumerable<DisputeListItemDto>>> GetSellerDisputes(
             int sellerId)
        {
            if (sellerId <= 0)
            {
                return BadRequest("sellerId không hợp lệ");
            }

            var disputes = await _service.GetDisputesBySellerAsync(sellerId);

            return Ok(disputes);
        }
        [HttpGet("supporter")]
        public async Task<ActionResult<IEnumerable<DisputeListItemDto>>> GetSupporterDisputes()
        {
            

            var disputes = await _service.GetDisputesForSupporterAsync();

            return Ok(disputes);
        }
        [HttpPut("respond")]
        public async Task<IActionResult> Respond([FromBody] RespondDisputeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

             _service.RespondDisputeAsync(dto);

            return Ok(new { message = "Cập nhật thành công" });
        }
    }
}
