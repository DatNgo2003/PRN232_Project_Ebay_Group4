using Microsoft.AspNetCore.Mvc;
using Frontend.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Frontend.Controllers
{
    public class AuthController : Controller
    {
        private readonly HttpClient _http;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IHttpClientFactory factory, ILogger<AuthController> logger)
        {
            _http = factory.CreateClient("API");
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> LoginApi([FromBody] UserLoginViewModel model)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("auth/login", model);

                var jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                LoginResponse? result;
                try
                {
                    result = JsonSerializer.Deserialize<LoginResponse>(jsonString, options);
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Không thể deserialize phản hồi đăng nhập. Phản hồi thô: {RawResponse}", jsonString);
                    return Json(new { success = false, message = "Lỗi máy chủ. Không thể đọc phản hồi." });
                }

                if (result == null)
                {
                    return Json(new { success = false, message = "Lỗi không xác định." });
                }

                if (!result.Success || result.Token == null || result.User == null)
                {
                    return Json(result);
                }

                HttpContext.Session.SetString("JwtToken", result.Token);
                HttpContext.Session.SetString("Username", result.User.Username ?? "User");
                // Set thêm userId và role
                HttpContext.Session.SetString("UserId", result.User.Id.ToString());
                HttpContext.Session.SetString("Role", result.User.Role ?? "User");

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi nghiêm trọng trong LoginApi");
                return Json(new { success = false, message = "Lỗi kết nối máy chủ." });
            }
        }
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public UserDto? User { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? AvatarUrl { get; set; }
    }
}