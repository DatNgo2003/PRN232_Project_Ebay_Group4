using Backend.DTOs.Requests;
using Backend.DTOs.Responses;
using Frontend.Helpers;
using Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Frontend.Controllers
{
    public class CheckoutController : Controller
    {
        private const string SessionKey = "Cart";
        private readonly ApiClientHelper _apiClient;
        private readonly ILogger<CheckoutController> _logger;
        private readonly IConfiguration _configuration;

        public CheckoutController(
            ApiClientHelper apiClient,
            ILogger<CheckoutController> logger,
            IConfiguration configuration)
        {
            _apiClient = apiClient;
            _logger = logger;
            _configuration = configuration;
        }

        private List<CartItemModel> GetCart()
        {
            var json = HttpContext.Session.GetString(SessionKey);
            if (string.IsNullOrEmpty(json)) return new List<CartItemModel>();
            try
            {
                return JsonSerializer.Deserialize<List<CartItemModel>>(json) ?? new List<CartItemModel>();
            }
            catch
            {
                return new List<CartItemModel>();
            }
        }

        private void ClearCart() => HttpContext.Session.Remove(SessionKey);

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cart = GetCart();
            if (cart.Count == 0)
            {
                TempData["CartMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            var viewItems = new List<CartLineViewModel>();
            foreach (var item in cart)
            {
                var response = await _apiClient.GetAsync($"products/{item.ProductId}");
                if (!response.IsSuccessStatusCode) continue;

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var product = JsonSerializer.Deserialize<ProductDetailDto>(content, options);
                if (product == null) continue;

                viewItems.Add(new CartLineViewModel
                {
                    ProductId = product.Id,
                    Title = product.Title ?? "(Không có tên)",
                    Image = product.Images,
                    SellerName = product.SellerName,
                    UnitPrice = product.Price ?? 0,
                    Quantity = item.Quantity
                });
            }

            ViewBag.Addresses = await GetMyAddressesAsync();
            ViewBag.PayPalClientId = _configuration["PayPal:ClientId"];
            ViewBag.PayPalCurrency = _configuration["PayPal:Currency"] ?? "USD";

            return View(viewItems);
        }

        private async Task<List<AddressDto>> GetMyAddressesAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync("address/my-addresses");
                if (!response.IsSuccessStatusCode) return new List<AddressDto>();

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<AddressDto>>(content, options) ?? new List<AddressDto>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể lấy danh sách địa chỉ của user");
                return new List<AddressDto>();
            }
        }

        public class CalculateTotalRequestModel
        {
            public int? AddressId { get; set; }
            public string? CouponCode { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> CalculateTotal([FromBody] CalculateTotalRequestModel model)
        {
            var cart = GetCart();
            if (cart.Count == 0)
                return BadRequest(new { message = "Giỏ hàng đang trống." });

            if (model.AddressId == null)
                return BadRequest(new { message = "Vui lòng chọn địa chỉ giao hàng trước." });

            var payload = new
            {
                Items = cart.Select(c => new { ProductId = c.ProductId, Quantity = c.Quantity }),
                AddressId = model.AddressId,
                CouponCode = string.IsNullOrWhiteSpace(model.CouponCode) ? null : model.CouponCode
            };

            try
            {
                var response = await _apiClient.PostAsync("order/calculate-total", payload);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, content);

                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tính tổng tiền giỏ hàng");
                return StatusCode(500, new { message = "Không thể tính tổng tiền lúc này." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(int? addressId, string? couponCode)
        {
            var cart = GetCart();
            if (cart.Count == 0)
            {
                TempData["CartMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            try
            {
                var payload = new CartCheckoutRequestDto
                {
                    Items = cart.Select(c => new OrderItemRequestDto { ProductId = c.ProductId, Quantity = c.Quantity }).ToList(),
                    PaymentMethod = "COD",
                    AddressId = addressId,
                    CouponCode = string.IsNullOrWhiteSpace(couponCode) ? null : couponCode
                };

                var response = await _apiClient.PostAsync("order/cart-checkout", payload);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Lỗi khi checkout giỏ hàng: {Content}", content);
                    TempData["CheckoutError"] = "Không thể đặt hàng. Vui lòng kiểm tra lại địa chỉ và thử lại.";
                    return RedirectToAction("Index");
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var wrapper = JsonSerializer.Deserialize<PlaceOrderResponseWrapper>(content, options);

                ClearCart();

                return RedirectToAction("Confirmation", new { orderId = wrapper?.Checkout?.OrderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi nghiêm trọng khi đặt hàng từ giỏ hàng");
                TempData["CheckoutError"] = "Có lỗi xảy ra, vui lòng thử lại.";
                return RedirectToAction("Index");
            }
        }

        private class PlaceOrderResponseWrapper
        {
            public CartCheckoutResponseDto? Checkout { get; set; }
        }

        public class PayPalCartCreateOrderProxyRequest
        {
            public int? AddressId { get; set; }
            public string? CouponCode { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayPalOrder([FromBody] PayPalCartCreateOrderProxyRequest request)
        {
            var cart = GetCart();
            if (cart.Count == 0)
                return BadRequest(new { message = "Giỏ hàng đang trống." });

            try
            {
                var payload = new PayPalCartCreateOrderRequestDto
                {
                    Items = cart.Select(c => new OrderItemRequestDto { ProductId = c.ProductId, Quantity = c.Quantity }).ToList(),
                    AddressId = request.AddressId,
                    CouponCode = request.CouponCode
                };

                var response = await _apiClient.PostAsync("paypal/create-cart-order", payload);
                var content = await response.Content.ReadAsStringAsync();
                return new ContentResult
                {
                    StatusCode = (int)response.StatusCode,
                    ContentType = "application/json",
                    Content = content
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo PayPal order cho giỏ hàng");
                return StatusCode(502, new { message = "Không thể kết nối PayPal." });
            }
        }

        public class PayPalCaptureProxyRequest
        {
            public int OrderId { get; set; }
            public string PayPalOrderId { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> CapturePayPalOrder([FromBody] PayPalCaptureProxyRequest request)
        {
            try
            {
                var response = await _apiClient.PostAsync("paypal/capture-order", request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    ClearCart();
                }

                return new ContentResult
                {
                    StatusCode = (int)response.StatusCode,
                    ContentType = "application/json",
                    Content = content
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi capture PayPal order {PayPalOrderId}", request.PayPalOrderId);
                return StatusCode(502, new { message = "Không thể kết nối PayPal." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(int orderId)
        {
            try
            {
                var response = await _apiClient.GetAsync("order/my-history");
                if (!response.IsSuccessStatusCode)
                {
                    return View("Error");
                }

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var history = JsonSerializer.Deserialize<List<PurchaseHistoryItemDto>>(content, options) ?? new List<PurchaseHistoryItemDto>();

                var orderItems = history.Where(h => h.OrderId == orderId).ToList();
                if (orderItems.Count == 0)
                {
                    return View("NotFound");
                }

                return View(orderItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang xác nhận đơn hàng #{OrderId}", orderId);
                return View("Error");
            }
        }
    }
}