using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticController : ControllerBase
    {
        private readonly CloneEbayDbContext _context;

        public DiagnosticController(CloneEbayDbContext context)
        {
            _context = context;
        }

        [HttpGet("db")]
        public async Task<IActionResult> GetDbStatus()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                var msgCount = await _context.Messages.CountAsync();
                var userCount = await _context.Users.CountAsync();
                return Ok(new { canConnect, msgCount, userCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
            }
        }
    }
}
