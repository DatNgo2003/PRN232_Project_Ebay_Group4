
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Frontend.Controllers
{
	public class SellerToBuyerController : Controller
	{
		private readonly HttpClient _httpClient;

		public SellerToBuyerController(IHttpClientFactory factory)
		{
			_httpClient = factory.CreateClient();
		}
		public async Task<IActionResult> GetSellerCommentByBuyerId(int? buyerId=1)
		{
			var urlBuyer = $"http://localhost:5236/api/SellerToBuyer/{buyerId}";
			var responseBuyer = await _httpClient.GetAsync(urlBuyer);
			var json = await responseBuyer.Content.ReadAsStringAsync();
			var sellerCommentToBuyer = JsonSerializer.Deserialize<List<SellerToBuyerReview>>(json, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			});

			return View(sellerCommentToBuyer);
		}
	}
}
