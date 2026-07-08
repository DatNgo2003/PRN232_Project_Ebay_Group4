using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Backend.Controllers
{
	[ApiController]
	[Route("api/[Controller]")]
	public class TestNginxController : Controller
	{
		[HttpGet]
		public IActionResult GetHostName()
		{
			var hostname = Dns.GetHostName();
			var ip = Dns.GetHostAddresses(hostname).FirstOrDefault()?.ToString();

			return Ok(new
			{
				Message = $"Hello MY FEN from {hostname}",
				Hostname = hostname,
				IP = ip,
				Time = DateTime.Now
			});
		}
	}
}
