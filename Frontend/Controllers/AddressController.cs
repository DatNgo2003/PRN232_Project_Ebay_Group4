using Backend.DTOs.Requests;
using Frontend.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    public class AddressController : Controller
    {
        private readonly ApiClientHelper _apiClient;
        private readonly ILogger<AddressController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public AddressController(ApiClientHelper apiClient, ILogger<AddressController> logger, IHttpClientFactory httpClientFactory)
        {
            _apiClient = apiClient;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Create(int? returnProductId)
        {
            ViewBag.ReturnProductId = returnProductId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAddressDto dto, int? returnProductId)
        {
            try
            {
                var response = await _apiClient.PostAsync("address", dto);

                if (response.IsSuccessStatusCode)
                {
                    if (returnProductId.HasValue)
                    {
                        return RedirectToAction("Detail", "Product", new { id = returnProductId.Value });
                    }
                    return RedirectToAction("Index", "Home");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Auth");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Lỗi API khi tạo address: {response.ReasonPhrase}. Content: {errorContent}");

                ModelState.AddModelError("", "Không thể lưu địa chỉ. Vui lòng kiểm tra lại thông tin.");
                ViewBag.ReturnProductId = returnProductId;
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi nghiêm trọng khi tạo địa chỉ");
                return View("Error");
            }
        }

        // >>> THÊM MỚI: lấy danh sách Tỉnh/Thành phố từ API công khai <
        [HttpGet]
        public async Task<IActionResult> GetProvinces()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var json = await client.GetStringAsync("https://provinces.open-api.vn/api/p/");
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể lấy danh sách tỉnh/thành");
                return Content("[]", "application/json");
            }
        }

        // >>> THÊM MỚI: lấy danh sách Quận/Huyện theo mã Tỉnh <
        [HttpGet]
        public async Task<IActionResult> GetDistricts(int provinceCode)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var json = await client.GetStringAsync($"https://provinces.open-api.vn/api/p/{provinceCode}?depth=2");
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể lấy danh sách quận/huyện");
                return Content("{}", "application/json");
            }
        }
    }
}