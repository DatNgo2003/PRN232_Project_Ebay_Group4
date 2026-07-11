using Backend.DTOs.Requests;
using Backend.Exceptions;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        private string GetUsernameFromToken()
        {
            return User.Identity?.Name ?? throw new InvalidOperationException("User is not authenticated.");
        }

        [HttpGet("my-addresses")]
        public async Task<IActionResult> GetMyAddresses()
        {
            try
            {
                var username = GetUsernameFromToken();
                var addresses = await _addressService.GetMyAddressesAsync(username);
                return Ok(addresses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAddress([FromBody] CreateAddressDto dto)
        {
            try
            {
                var username = GetUsernameFromToken();
                var created = await _addressService.CreateAddressAsync(username, dto);
                return Ok(created);
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}