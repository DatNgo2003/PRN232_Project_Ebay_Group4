using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    public class DisputeV2Controller : Controller
    {
        [Route("dispute/create")]
        public IActionResult Create()
        {
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
            {
                // Nếu chưa đăng nhập (không có token), đá về trang Login
                return RedirectToAction("Login", "Auth");
            }

            // "Tiêm" token vào trang CSHTML
            ViewData["ApiToken"] = token;
            var correlationId = HttpContext.TraceIdentifier;

            ViewData["CorrelationId"] = correlationId;

            // Render trang Views/Dispute/Create.cshtml
            return View();

        }
    }
}
