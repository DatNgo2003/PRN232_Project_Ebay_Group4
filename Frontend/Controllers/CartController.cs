using Backend.DTOs.Responses;
using Frontend.Helpers;
using Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Frontend.Controllers
{
    public class CartController : Controller
    {
        private const string SessionKey = "Cart";
        private readonly ApiClientHelper _apiClient;
        private readonly ILogger<CartController> _logger;

        public CartController(ApiClientHelper apiClient, ILogger<CartController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
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

        private void SaveCart(List<CartItemModel> cart)
        {
            HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(cart));
        }

        [HttpPost]
        public IActionResult Add(int productId, int quantity = 1)
        {
            if (productId <= 0) return BadRequest(new { message = "Sản phẩm không hợp lệ." });
            if (quantity <= 0) quantity = 1;

            var cart = GetCart();
            var existing = cart.FirstOrDefault(c => c.ProductId == productId);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItemModel { ProductId = productId, Quantity = quantity });
            }

            SaveCart(cart);

            return Ok(new { message = "Đã thêm vào giỏ hàng.", itemCount = cart.Sum(c => c.Quantity) });
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cart = GetCart();
            var viewItems = new List<CartLineViewModel>();

            foreach (var item in cart)
            {
                try
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
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không thể tải sản phẩm {ProductId} trong giỏ hàng", item.ProductId);
                }
            }

            return View(viewItems);
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCart();
            var existing = cart.FirstOrDefault(c => c.ProductId == productId);
            if (existing != null)
            {
                existing.Quantity = quantity <= 0 ? 1 : quantity;
                SaveCart(cart);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Remove(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(c => c.ProductId == productId);
            SaveCart(cart);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(SessionKey);
            return Ok();
        }
    }
}