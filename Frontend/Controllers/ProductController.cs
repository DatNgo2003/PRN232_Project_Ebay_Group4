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
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var productDetail = JsonSerializer.Deserialize<ProductDetailDto>(content, options);

                    if (productDetail == null)
                    {
                        _logger.LogWarning($"Không thể deserialize ProductDetailDto cho ID: {id}");
                        return View("NotFound");
                    }

                    ViewBag.Addresses = await GetMyAddressesAsync();
                    ViewBag.Coupons = await GetAvailableCouponsAsync(id); // >>> THÊM MỚI <

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

        private async Task<List<CouponDto>> GetAvailableCouponsAsync(int productId)
        {
            try
            {
                var response = await _apiClient.GetAsync($"coupon/available?productId={productId}");
                if (!response.IsSuccessStatusCode)
                {
                    return new List<CouponDto>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var coupons = JsonSerializer.Deserialize<List<CouponDto>>(content, options);
                return coupons ?? new List<CouponDto>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể lấy danh sách coupon khả dụng");
                return new List<CouponDto>();
            }
        }

        private async Task<List<AddressDto>> GetMyAddressesAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync("address/my-addresses");
                if (!response.IsSuccessStatusCode)
                {
                    return new List<AddressDto>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var addresses = JsonSerializer.Deserialize<List<AddressDto>>(content, options);
                return addresses ?? new List<AddressDto>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể lấy danh sách địa chỉ của user");
                return new List<AddressDto>();
            }
        }

        public class CalculateTotalRequestModel
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; } = 1;
            public int? AddressId { get; set; }
            public string? CouponCode { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> CalculateTotal([FromBody] CalculateTotalRequestModel model)
        {
            if (model.AddressId == null)
            {
                return BadRequest(new { message = "Vui lòng chọn địa chỉ giao hàng trước." });
            }

            if (model.Quantity <= 0) model.Quantity = 1;

            var payload = new
            {
                Items = new[]
                {
                    new { ProductId = model.ProductId, Quantity = model.Quantity }
                },
                AddressId = model.AddressId,
                CouponCode = string.IsNullOrWhiteSpace(model.CouponCode) ? null : model.CouponCode
            };

            try
            {
                var response = await _apiClient.PostAsync("order/calculate-total", payload);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, content);
                }

                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi calculate-total cho ProductId={ProductId}", model.ProductId);
                return StatusCode(500, new { message = "Không thể tính tổng tiền lúc này." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Buy(
            int id,
            string? paymentMethod,
            int? addressId,
            int quantity = 1,
            string? couponCode = null)
        {
            try
            {
                var checkoutRequest = new Backend.DTOs.Requests.QuickBuyCheckoutRequestDto
                {
                    ProductId = id,
                    PaymentMethod = paymentMethod,
                    AddressId = addressId,
                    Quantity = quantity,
                    CouponCode = couponCode
                };

                var response = await _apiClient.PostAsync("order/quick-buy", checkoutRequest);

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

                TempData["BuyError"] = "Không thể đặt hàng. Vui lòng kiểm tra địa chỉ giao hàng và thử lại.";
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