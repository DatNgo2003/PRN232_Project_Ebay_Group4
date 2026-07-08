using Backend.Models;
using Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Frontend.Controllers
{
	public class FeedbackController : Controller
	{
		private readonly HttpClient _httpClient;

		public FeedbackController(IHttpClientFactory factory)
		{
			_httpClient = factory.CreateClient();
		}
		public async Task<IActionResult> OverviewFeedback(int? sellerId = 5)
		{

			var urlSeller = $"http://localhost:5236/api/Feedback/seller/{sellerId}";
			var responseOrder = await _httpClient.GetAsync(urlSeller);
			var json = await responseOrder.Content.ReadAsStringAsync();
			var user = JsonSerializer.Deserialize<User>(json, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			});
			return View(user);
		}

		public async Task<IActionResult> FormFeedback(string? result, int? id = 3)
		{

			if(result != null)
			{
				ViewBag.UpdatedResult = result;
			}
			var urlOrder = $"http://localhost:5236/api/Feedback/order/{id}";
			var responseOrder = await _httpClient.GetAsync(urlOrder);
			var json = await responseOrder.Content.ReadAsStringAsync();
			var order = JsonSerializer.Deserialize<OrderTable>(json, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			});

			FormFeedbackViewModel model = new FormFeedbackViewModel();
			model.OrderTable = order;
			model.ReceiverId = order.OrderItems.ToList()[0].Product.Seller.Id;
			if(order.IsCommented == true)
			{
				model.PositiveRate = (int)order.Feedbacks.ToList()[0].PositiveRate;
				model.Comment = order.Feedbacks.ToList()[0].Comment;
				model.DeliveryOnTime = (int)order.Feedbacks.ToList()[0].DetailFeedbacks.ToList()[0].DeliveryOnTime;
				model.ExactSame = (int)order.Feedbacks.ToList()[0].DetailFeedbacks.ToList()[0].ExactSame;
				model.Communication = (int)order.Feedbacks.ToList()[0].DetailFeedbacks.ToList()[0].Communication;
			}
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> FormFeedback(FormFeedbackViewModel model)
		{
			var urlOrder = $"http://localhost:5236/api/Feedback/order/addfeedback/{model.OrderTableId}";
			var responseOrder = await _httpClient.PostAsJsonAsync(urlOrder, model);
			if (responseOrder.IsSuccessStatusCode)
			{
				return RedirectToAction("FormFeedback", new { id = model.OrderTableId, result = "Add comment feedback successfully" });
			}

			return RedirectToAction("FormFeedback", new { id = model.OrderTableId, result = "Can not save feedback" });
		}
	}
}
