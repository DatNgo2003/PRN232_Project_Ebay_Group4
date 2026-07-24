using Frontend.Helpers;
using Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Frontend.Controllers
{
    public class SalesController : Controller
    {
        private readonly ApiClientHelper _apiClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SalesController(ApiClientHelper apiClient, IHttpContextAccessor httpContextAccessor)
        {
            _apiClient = apiClient;
            _httpContextAccessor = httpContextAccessor;
        }

        private bool IsSeller()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("Role") != "buyer";
        }

        private bool IsGuest()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("Role") == null;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsSeller() || IsGuest())
            {
                return RedirectToAction("Index", "Home");
            }

            var salesHistory = new List<SellerSalesOrderDto>();

            try
            {
                var response = await _apiClient.GetAsync("Order/seller/my-sales");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    salesHistory = JsonSerializer.Deserialize<List<SellerSalesOrderDto>>(jsonString, options);
                }
                else
                {
                    TempData["Error"] = $"Không thể tải lịch sử bán hàng. Lỗi: {response.ReasonPhrase}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi kết nối API: {ex.Message}";
            }

            return View(salesHistory);
        }

        /// <summary>
        /// GET /Sales/Details/{id} — Seller views order detail and can update shipping status.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (!IsSeller() || IsGuest())
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // Fetch sales history and find the specific order
                var response = await _apiClient.GetAsync("Order/seller/my-sales");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Không thể tải thông tin đơn hàng.";
                    return RedirectToAction("Index");
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var salesHistory = JsonSerializer.Deserialize<List<SellerSalesOrderDto>>(jsonString, options);
                var order = salesHistory?.FirstOrDefault(o => o.OrderId == id);

                if (order == null)
                {
                    TempData["Error"] = $"Không tìm thấy đơn hàng #{id} hoặc bạn không có quyền xem.";
                    return RedirectToAction("Index");
                }

                // Fetch available carriers
                var carriersResponse = await _apiClient.GetAsync("shipping/carriers");
                if (carriersResponse.IsSuccessStatusCode)
                {
                    var carriersJson = await carriersResponse.Content.ReadAsStringAsync();
                    ViewBag.Carriers = JsonSerializer.Deserialize<List<ShippingCarrierDto>>(carriersJson, options);
                }
                else
                {
                    ViewBag.Carriers = new List<ShippingCarrierDto>();
                }

                return View(order);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi kết nối API: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// POST /Sales/UpdateShippingStatus — Seller updates order shipping status.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateShippingStatus(int orderId, string status)
        {
            if (!IsSeller() || IsGuest())
            {
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                TempData["Error"] = "Vui lòng chọn trạng thái.";
                return RedirectToAction("Details", new { id = orderId });
            }

            try
            {
                var payload = new { Status = status };
                var response = await _apiClient.PutAsync($"Order/{orderId}/shipping-status", payload);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn hàng #{orderId} thành '{status}' thành công.";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Không thể cập nhật trạng thái: {errorContent}";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi kết nối API: {ex.Message}";
            }

            return RedirectToAction("Details", new { id = orderId });
        }
    }

    /// <summary>
    /// DTO for carrier info returned by /api/shipping/carriers
    /// </summary>
    public class ShippingCarrierDto
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
