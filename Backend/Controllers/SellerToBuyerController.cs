using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class SellerToBuyerController : Controller
	{

		private readonly CloneEbayDbContext _context;

		public SellerToBuyerController(CloneEbayDbContext context)
		{
			_context = context;
		}

		[HttpGet("{buyerId}")]
		public IActionResult GetSellerCommentByBuyerId(int? buyerId)
		{
			var sellerComment = _context.SellerToBuyerReviews.Where(sc => sc.BuyerId == buyerId).ToList();
			return Ok(sellerComment);
		}
	}
}
