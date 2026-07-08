using Frontend.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Backend.DTOs.Responses;

namespace Frontend.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApiClientHelper _apiClient;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ApiClientHelper apiClient, ILogger<ProductController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var response = await _apiClient.GetAsync($"products/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var productDetail = JsonSerializer.Deserialize<ProductDetailDto>(content, options);

                    if (productDetail == null)
                    {
                        _logger.LogWarning($"Không thể deserialize ProductDetailDto cho ID: {id}");
                        return View("NotFound");
                    }

                    return View(productDetail);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning($"API không tìm thấy sản phẩm với ID: {id}");
                    return View("NotFound");
                }

                _logger.LogError($"Lỗi API khi gọi products/{id}: {response.ReasonPhrase}");
                return View("Error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi nghiêm trọng khi lấy chi tiết sản phẩm ID: {id}");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Buy(int id)
        {
            try
            {
                var response = await _apiClient.PostEmptyAsync($"order/quick-buy?productId={id}");

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Purchase");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Auth");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Lỗi API khi gọi order/quick-buy: {response.ReasonPhrase}. Content: {errorContent}");

                return RedirectToAction("Detail", new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi nghiêm trọng khi Buy sản phẩm ID: {id}");
                return View("Error");
            }
        }
    }
}