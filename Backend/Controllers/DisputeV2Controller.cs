using Backend.DTOs;
using Backend.DTOs.Requests;
using Backend.Services;
using Backend.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    [Authorize] // Yêu cầu xác thực cho tất cả
    public class DisputeV2Controller : ControllerBase
    {
        private readonly IDisputeServiceV2 _disputeService;

        public DisputeV2Controller(IDisputeServiceV2 disputeService)
        {
            _disputeService = disputeService;
        }

        // 1. Endpoint lấy đơn hàng: GET /api/dispute/disputable
        [HttpGet("disputable")]
        public async Task<IActionResult> GetDisputableOrders()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int currentUserId))
            {
                return Unauthorized("Token không hợp lệ.");
            }

            var orders = await _disputeService.GetDisputableOrdersAsync(currentUserId);
            return Ok(orders);
        }

        // 2. Endpoint tạo khiếu nại: POST /api/dispute
        [HttpPost]
        public async Task<IActionResult> CreateDispute([FromBody] DisputeCreateDto dto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int currentUserId))
            {
                return Unauthorized("Token không hợp lệ.");
            }

            try
            {
                var dispute = await _disputeService.CreateDisputeAsync(dto, currentUserId);
                return CreatedAtAction(nameof(CreateDispute), new { id = dispute.Id }, dispute);
            }
       
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, $"Lỗi máy chủ nội bộ: {ex.Message}");
            }
        }
    }
}