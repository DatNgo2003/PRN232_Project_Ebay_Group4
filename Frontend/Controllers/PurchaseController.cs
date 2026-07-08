using Backend.DTOs.Responses;
using Frontend.Helpers;
using Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Frontend.Controllers
{
    public class PurchaseController : Controller
    {
        private readonly ApiClientHelper _apiClient;
        private readonly ILogger<PurchaseController> _logger;

        public PurchaseController(ApiClientHelper apiClient, ILogger<PurchaseController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _apiClient.GetAsync("order/my-history");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var history = JsonSerializer.Deserialize<List<PurchaseHistoryItemDto>>(content, options);

                    return View(history ?? new List<PurchaseHistoryItemDto>());
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return Redirect("/Auth/Login");
                }

                _logger.LogError($"Lỗi API khi gọi order/my-history: {response.ReasonPhrase}");
                return View(new List<PurchaseHistoryItemDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi nghiêm trọng khi lấy Purchase History");
                return View("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> LeaveReview(int id)
        {
            try
            {
                var response = await _apiClient.GetAsync($"products/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        return RedirectToAction("Login", "Auth");

                    return View("NotFound");
                }

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var productDetail = JsonSerializer.Deserialize<ProductDetailDto>(content, options);

                if (productDetail == null)
                {
                    return View("NotFound");
                }

                var viewModel = new LeaveReviewViewModel
                {
                    ProductId = productDetail.Id,
                    ProductTitle = productDetail.Title,
                    ProductImage = productDetail.Images
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi tải trang LeaveReview cho ProductId: {id}");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> LeaveReview(LeaveReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var dto = new Backend.DTOs.Requests.CreateReviewDto
                {
                    ProductId = model.ProductId,
                    Rating = model.Rating,
                    Comment = model.Comment
                };

                var response = await _apiClient.PostAsync("review", dto);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Auth");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Lỗi API khi gửi review: {errorContent}");
                ModelState.AddModelError(string.Empty, "Failed to submit review. You may have already reviewed this item.");

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi nghiêm trọng khi gửi LeaveReview");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
                return View(model);
            }
        }
    }
}