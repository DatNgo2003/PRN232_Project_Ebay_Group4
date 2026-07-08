using Backend.DTOs.Requests;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
	//http://localhost:5236/api/Feedback/seller/5
	[ApiController]
	[Route("api/[Controller]")]
	public class FeedbackController : Controller
	{
		private readonly CloneEbayDbContext _context;

		public FeedbackController(CloneEbayDbContext context)
		{
			_context = context;
		}

		[HttpGet("seller/{sellerId}")]
		public async Task<IActionResult> GetSellerById(int sellerId)
		{
			var seller = await _context.Users.Include(u => u.Feedbacks)
				.ThenInclude(f => f.DetailFeedbacks)
				.Include(u => u.Products)
				.ThenInclude(p => p.OrderItems)
				.ThenInclude(ot => ot.Order)
				.FirstOrDefaultAsync(u => u.Id == sellerId);

			//var negative = seller!.Feedbacks.Where(f => f.PositiveRate == -1).Count();
			//var neutral = seller!.Feedbacks.Where(f => f.PositiveRate == 0).Count();
			//var positive = seller!.Feedbacks.Where(f => f.PositiveRate == 1).Count();
			
				
			return Ok(seller);
		}


		[HttpGet("order/{orderId}")]
		public async Task<IActionResult> GetOrderById(int orderId)
		{
			var order = await _context.OrderTables
				.Include(o => o.Feedbacks)
				.ThenInclude(f => f.DetailFeedbacks)
				.Include(ot => ot.OrderItems)
				.ThenInclude(oi => oi.Product)
				.ThenInclude(p => p.Seller)
				.FirstOrDefaultAsync(o => o.Id == orderId);

			return Ok(order);
		}


		[HttpPost("order/addfeedback/{orderId}")]
		public async Task<IActionResult> AddFeedbackForOrder(int orderId, [FromBody] FeedbackDto feedbackDto)
		{
			decimal everageRate = (feedbackDto.Communication + feedbackDto.DeliveryOnTime + feedbackDto.ExactSame)/3;


			var feedback = new Feedback()
			{
				OrdersId = orderId,
				PositiveRate = feedbackDto.PositiveRate,
				AverageRating = everageRate,
				TotalReviews = 1,
				Comment = feedbackDto.Comment,
				SellerId = feedbackDto.ReceiverId,

			};

			await _context.Feedbacks.AddAsync(feedback);
			await _context.SaveChangesAsync();

			var feedbackDetail = new DetailFeedback() { 
				FeedbackId = feedback.Id,
				Communication = feedbackDto.Communication,
				DeliveryOnTime = feedbackDto.DeliveryOnTime,
				ExactSame = feedbackDto.ExactSame
			};
			await _context.DetailFeedbacks.AddAsync(feedbackDetail);
			await _context.SaveChangesAsync();


			var order = await _context.OrderTables.FirstOrDefaultAsync(o => o.Id == orderId);
			order.IsCommented = true;
			await _context.SaveChangesAsync();

			return Ok();
		}






	}
}
