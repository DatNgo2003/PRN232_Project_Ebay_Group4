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
            // 1. Kiểm tra vai trò (Role)
            if (!IsSeller() || IsGuest())
            {
                // Nếu không phải Seller, trả về trang Unauthorized hoặc trang chủ
                return RedirectToAction("Index", "Home");
            }

            // 2. Chuẩn bị model rỗng
            var salesHistory = new List<SellerSalesOrderDto>();

            try
            {
                // 3. Gọi API bằng ApiClientHelper
                var response = await _apiClient.GetAsync("Order/seller/my-sales");

                if (response.IsSuccessStatusCode)
                {
                    // 4. Đọc và Deserialize dữ liệu
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    salesHistory = JsonSerializer.Deserialize<List<SellerSalesOrderDto>>(jsonString, options);
                }
                else
                {
                    // Xử lý lỗi (ví dụ: hiển thị TempData, log lỗi)
                    TempData["Error"] = $"Không thể tải lịch sử bán hàng. Lỗi: {response.ReasonPhrase}";
                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ (ví dụ: API không chạy)
                TempData["Error"] = $"Lỗi kết nối API: {ex.Message}";
            }

            // 5. Trả về View với model (có thể rỗng nếu lỗi)
            return View(salesHistory);
        }
    }
}