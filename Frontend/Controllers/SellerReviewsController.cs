using Frontend.Helpers;
using Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Frontend.Controllers
{
    public class SellerReviewsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly string _apiBaseUrl = "http://localhost:5236/api";

        public SellerReviewsController(IHttpClientFactory factory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = factory.CreateClient();
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddAuthTokenToRequest()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");

            _httpClient.DefaultRequestHeaders.Authorization = null;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        [HttpGet]
        public IActionResult LeaveReview(int orderId)
        {
            var model = new SellerLeaveReviewViewModel
            {
                OrderId = orderId
            };
            return View("~/Views/Sales/LeaveReview.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> LeaveReview(SellerLeaveReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                AddAuthTokenToRequest();

                var url = $"{_apiBaseUrl}/seller-reviews";

                var dto = new { OrderId = model.OrderId, Comment = model.Comment };

                var response = await _httpClient.PostAsJsonAsync(url, dto);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Review saved successfully!";
                    return RedirectToAction("Index", "Sales");
                }

                ModelState.AddModelError(string.Empty, "An error occurred.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Internal error: {ex.Message}");
            }

			return RedirectToAction("Index", "Sales");
		}

        [HttpGet]
        public async Task<IActionResult> Received()
        {
            try
            {
                AddAuthTokenToRequest();

                var url = $"{_apiBaseUrl}/seller-reviews/received-as-buyer";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var reviews = JsonSerializer.Deserialize<List<ReviewsViewModel>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return View(reviews);
                }

                ViewData["ErrorMessage"] = "Failed to load reviews.";
                return View(new List<ReviewsViewModel>());
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Internal error: {ex.Message}";
                return View(new List<ReviewsViewModel>());
            }
        }
    }
}