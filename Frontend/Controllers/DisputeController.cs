using Backend.DTOs.Dispute;
using Backend.DTOs.Responses;
using Frontend.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace Frontend.Controllers
{
    public class DisputeController : Controller
    {
        private readonly ApiClientHelper _apiClient;
        private readonly ILogger<DisputeController> _logger;
        private readonly HttpClient _httpClient;
        public DisputeController(ApiClientHelper apiClient, ILogger<DisputeController> logger, IHttpClientFactory httpClientFactory)
        {
            _apiClient = apiClient;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("API");
        }
        [Route("dispute/buyer/{buyerId}")]
        [HttpGet]
        public async Task<IActionResult> BuyerDispute(
    int buyerId,
    int? statusFilter,          // 1,2,3,4 hoặc null = tất cả
    string? timeFilter          // "30", "60", "all"
)
        {
            try
            {
                if (buyerId <= 0)
                {
                    _logger.LogWarning("buyerId không hợp lệ: {buyerId}", buyerId);
                    return View(new List<DisputeListItemDto>());
                }

                // Gọi API: GET /api/disputes/buyer/{buyerId}
                var response = await _apiClient.GetAsync($"disputes/buyer/{buyerId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Lỗi gọi API: {status} {reason}",
                        response.StatusCode, response.ReasonPhrase);
                    return View(new List<DisputeListItemDto>());
                }

                var content = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var disputes = JsonSerializer.Deserialize<List<DisputeListItemDto>>(content, options)
                              ?? new List<DisputeListItemDto>();

                // ============ APPLY FILTER ============

                var query = disputes.AsQueryable();

                // 1. Filter theo status cụ thể (1,2,3,4)
                if (statusFilter.HasValue)
                {
                    if (statusFilter.Value == 1) // Chưa phản hồi
                    {
                        query = query.Where(d => d.Status == "1" || d.Status == "3");
                    }
                    else if (statusFilter.Value == 2) // Đã phản hồi
                    {
                        query = query.Where(d => d.Status == "2" || d.Status == "4");
                    }

                }


                // 3. Filter khoảng thời gian
                if (!string.IsNullOrWhiteSpace(timeFilter) && timeFilter != "all")
                {
                    DateTime fromDate;

                    if (timeFilter == "30")
                        fromDate = DateTime.UtcNow.AddDays(-30);
                    else if (timeFilter == "60")
                        fromDate = DateTime.UtcNow.AddDays(-60);
                    else
                        fromDate = DateTime.MinValue;

                    query = query.Where(d => d.SubmittedDate.HasValue &&
                                             d.SubmittedDate.Value >= fromDate);
                }

                // 4. Sắp xếp: status 1 & 3 (chưa phản hồi) lên trước
                var result = query
                    .ToList();

                // Lưu giá trị filter để view đánh dấu selected
                ViewBag.buyerId = buyerId;
                ViewBag.StatusFilter = statusFilter;
                ViewBag.TimeFilter = timeFilter;

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi nghiêm trọng khi lấy danh sách khiếu nại cho buyerId: {buyerId}", buyerId);
                return View("Error");
            }
        }

        [Route("dispute/seller/{sellerId}")]
        [HttpGet]
        public async Task<IActionResult> SellerDispute(int sellerId, int? statusFilter,          // 1,2,3,4 hoặc null = tất cả
    string? timeFilter)          // "30", "60", "all"
        {
            try
            {
                if (sellerId <= 0)
                {
                    _logger.LogWarning("buyerId không hợp lệ: {sellerId}", sellerId);
                    return View(new List<DisputeListItemDto>());
                }

                // Gọi API: GET /api/disputes/buyer/{buyerId}
                var response = await _apiClient.GetAsync($"disputes/seller/{sellerId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Lỗi gọi API: {status} {reason}",
                        response.StatusCode, response.ReasonPhrase);
                    return View(new List<DisputeListItemDto>());
                }

                var content = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var disputes = JsonSerializer.Deserialize<List<DisputeListItemDto>>(content, options)
                              ?? new List<DisputeListItemDto>();

                // ============ APPLY FILTER ============

                var query = disputes.AsQueryable();

                // 1. Filter theo status cụ thể (1,2,3,4)
                if (statusFilter.HasValue)
                {
                    if (statusFilter.Value == 1) // Chưa phản hồi
                    {
                        query = query.Where(d => d.Status == "1");
                    }
                    else if (statusFilter.Value == 2) // Đã phản hồi
                    {
                        query = query.Where(d => d.Status == "2");
                    }

                }


                // 3. Filter khoảng thời gian
                if (!string.IsNullOrWhiteSpace(timeFilter) && timeFilter != "all")
                {
                    DateTime fromDate;

                    if (timeFilter == "30")
                        fromDate = DateTime.UtcNow.AddDays(-30);
                    else if (timeFilter == "60")
                        fromDate = DateTime.UtcNow.AddDays(-60);
                    else
                        fromDate = DateTime.MinValue;

                    query = query.Where(d => d.SubmittedDate.HasValue &&
                                             d.SubmittedDate.Value >= fromDate);
                }

                // 4. Sắp xếp: status 1 & 3 (chưa phản hồi) lên trước
                var result = query
                    .ToList();

                // Lưu giá trị filter để view đánh dấu selected
                ViewBag.sellerId = sellerId;
                ViewBag.StatusFilter = statusFilter;
                ViewBag.TimeFilter = timeFilter;

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi nghiêm trọng khi lấy danh sách khiếu nại cho buyerId: {sellerId}", sellerId);
                return View("Error");
            }

        }
        [Route("dispute/supporter")]
        [HttpGet]
        public async Task<IActionResult> SupporterDispute( int? statusFilter, string? timeFilter)          // "30", "60", "all"
        {
            
                // Gọi API: GET /api/disputes/buyer/{buyerId}
                var response = await _apiClient.GetAsync($"disputes/supporter");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Lỗi gọi API: {status} {reason}",
                        response.StatusCode, response.ReasonPhrase);
                    return View(new List<DisputeListItemDto>());
                }

                var content = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var disputes = JsonSerializer.Deserialize<List<DisputeListItemDto>>(content, options)
                              ?? new List<DisputeListItemDto>();

                // ============ APPLY FILTER ============

                var query = disputes.AsQueryable();

                // 1. Filter theo status cụ thể (1,2,3,4)
                if (statusFilter.HasValue)
                {
                    if (statusFilter.Value == 1) // Chưa phản hồi
                    {
                        query = query.Where(d => d.Status == "3");
                    }
                    else if (statusFilter.Value == 2) // Đã phản hồi
                    {
                        query = query.Where(d => d.Status == "4");
                    }

                }


                // 3. Filter khoảng thời gian
                if (!string.IsNullOrWhiteSpace(timeFilter) && timeFilter != "all")
                {
                    DateTime fromDate;

                    if (timeFilter == "30")
                        fromDate = DateTime.UtcNow.AddDays(-30);
                    else if (timeFilter == "60")
                        fromDate = DateTime.UtcNow.AddDays(-60);
                    else
                        fromDate = DateTime.MinValue;

                    query = query.Where(d => d.SubmittedDate.HasValue &&
                                             d.SubmittedDate.Value >= fromDate);
                }

                // 4. Sắp xếp: status 1 & 3 (chưa phản hồi) lên trước
                var result = query
                    .ToList();

                ViewBag.StatusFilter = statusFilter;
                ViewBag.TimeFilter = timeFilter;

                return View(result);
            

        }

        [HttpPost]
        public async Task<IActionResult> Respond(RespondDisputeDto model)
        {
            _logger.LogInformation("Bắt đầu lưu phản hồi khiếu nại. DisputeId = {Id}", model.Id);
            ModelState.Remove(nameof(RespondDisputeDto.SolvedDate));
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState không hợp lệ khi lưu phản hồi. DisputeId = {Id}. Lỗi: {@Errors}",
                    model.Id, ModelState);
                TempData["Error"] = "Dữ liệu không hợp lệ.";
                return RedirectToAction("SellerDispute", new { sellerId = model.SellerId });
            }

            if (string.IsNullOrEmpty(model.Resolution))
            {
                _logger.LogWarning("Không chọn Resolution khi gửi phản hồi. DisputeId = {Id}", model.Id);
                TempData["Error"] = "Bạn phải chọn hành động phản hồi.";
                return RedirectToAction("SellerDispute", new { sellerId = model.SellerId });
            }

            // Set theo yêu cầu nghiệp vụ
            model.status = "2";
            model.SolvedDate = DateTime.Now;

            

            try
            {
                _logger.LogInformation("Gọi API cập nhật dispute. DisputeId = {Id}, Payload = {@Model}",
                    model.Id, model);

                var response = await _httpClient.PutAsJsonAsync("/api/disputes/respond", model);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Cập nhật dispute thành công. DisputeId = {Id}", model.Id);
                    TempData["Success"] = "Cập nhật phản hồi khiếu nại thành công.";
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();

                    _logger.LogError(
                        "Gọi API cập nhật dispute thất bại. DisputeId = {Id}, StatusCode = {StatusCode}, Body = {Body}",
                        model.Id, (int)response.StatusCode, errorBody);

                    TempData["Error"] = "Cập nhật thất bại từ API. Vui lòng thử lại.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception khi gọi API cập nhật dispute. DisputeId = {Id}", model.Id);
                TempData["Error"] = "Có lỗi hệ thống khi cập nhật. Vui lòng thử lại sau.";
            }

            return RedirectToAction("SellerDispute", new { sellerId = model.SellerId });
        }
        [HttpPost]
        public async Task<IActionResult> Respond2(RespondDisputeDto model)
        {
            _logger.LogInformation("Bắt đầu lưu phản hồi khiếu nại. DisputeId = {Id}", model.Id);
            ModelState.Remove(nameof(RespondDisputeDto.SolvedDate));
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState không hợp lệ khi lưu phản hồi. DisputeId = {Id}. Lỗi: {@Errors}",
                    model.Id, ModelState);
                TempData["Error"] = "Dữ liệu không hợp lệ.";
                return RedirectToAction("SupporterDispute");
            }

            if (string.IsNullOrEmpty(model.Resolution))
            {
                _logger.LogWarning("Không chọn Resolution khi gửi phản hồi. DisputeId = {Id}", model.Id);
                TempData["Error"] = "Bạn phải chọn hành động phản hồi.";
                return RedirectToAction("SupporterDispute");
            }

            // Set theo yêu cầu nghiệp vụ
            model.status = "4";
            model.SolvedDate = DateTime.Now;



            try
            {
                _logger.LogInformation("Gọi API cập nhật dispute. DisputeId = {Id}, Payload = {@Model}",
                    model.Id, model);

                var response = await _httpClient.PutAsJsonAsync("/api/disputes/respond", model);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Cập nhật dispute thành công. DisputeId = {Id}", model.Id);
                    TempData["Success"] = "Cập nhật phản hồi khiếu nại thành công.";
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();

                    _logger.LogError(
                        "Gọi API cập nhật dispute thất bại. DisputeId = {Id}, StatusCode = {StatusCode}, Body = {Body}",
                        model.Id, (int)response.StatusCode, errorBody);

                    TempData["Error"] = "Cập nhật thất bại từ API. Vui lòng thử lại.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception khi gọi API cập nhật dispute. DisputeId = {Id}", model.Id);
                TempData["Error"] = "Có lỗi hệ thống khi cập nhật. Vui lòng thử lại sau.";
            }

            return RedirectToAction("SupporterDispute");
        }
    }
}
