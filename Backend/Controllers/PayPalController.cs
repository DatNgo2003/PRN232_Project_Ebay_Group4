using Backend.DTOs.Requests;
using Backend.Exceptions;
using Backend.Services;
using Backend.Services.PaymentGateways;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Backend.Configuration;
using System.Security.Claims;

namespace Backend.Controllers;

[ApiController]
[Route("api/paypal")]
[Authorize]
[EnableRateLimiting("payment_shipping")]
public sealed class PayPalController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IPaymentGatewayFactory _paymentGatewayFactory;
    private readonly ITransactionLogger _txLogger;
    private readonly PayPalOptions _paypalOptions;
    private readonly ILogger<PayPalController> _logger;

    public PayPalController(
        IOrderService orderService,
        IPaymentGatewayFactory paymentGatewayFactory,
        ITransactionLogger txLogger,
        IOptions<PayPalOptions> paypalOptions,
        ILogger<PayPalController> logger)
    {
        _orderService = orderService;
        _paymentGatewayFactory = paymentGatewayFactory;
        _txLogger = txLogger;
        _paypalOptions = paypalOptions.Value;
        _logger = logger;
    }

    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder(
        [FromBody] PayPalCreateOrderRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.ProductId <= 0)
            return BadRequest(new { message = "ProductId is required." });

        var txId = _txLogger.StartTransaction("Payment.PayPal", "CreateOrder",
            new { request.ProductId, request.Quantity });
        int? localOrderId = null;

        try
        {
            var username = GetUsernameFromToken();
            var checkout = await _orderService.CreateQuickBuyOrderAsync(
                username,
                request.ProductId,
                "PayPal",
                request.AddressId,
                request.Quantity,
                request.CouponCode,
                request.CarrierKey);

            if (checkout == null)
                return BadRequest(new { message = "Unable to prepare the order." });

            localOrderId = checkout.OrderId;

            var gateway = _paymentGatewayFactory.Resolve("PayPal");
            var initiation = await gateway.InitiateAsync(
                new PaymentInitiationRequest(
                    checkout.TotalAmount,
                    _paypalOptions.Currency,
                    checkout.OrderId.ToString(),
                    $"Order #{checkout.OrderId}"),
                cancellationToken);

            if (!initiation.Success)
            {
                await _orderService.CancelOrderAsync(checkout.OrderId);
                _txLogger.LogFailure(txId, "Payment.PayPal", "CreateOrder",
                    new InvalidOperationException("PayPal initiation failed."), new { checkout.OrderId });
                return StatusCode(502, new { message = "Không thể tạo thanh toán PayPal. Vui lòng thử lại." });
            }

            var attached = await _orderService.AttachPayPalOrderAsync(
                username,
                checkout.OrderId,
                initiation.ProviderTransactionId);

            if (!attached)
            {
                await _orderService.CancelOrderAsync(checkout.OrderId);
                _txLogger.LogFailure(txId, "Payment.PayPal", "CreateOrder",
                    new InvalidOperationException("Không thể gắn PayPal order vào đơn hàng nội bộ."),
                    new { checkout.OrderId });
                return BadRequest(new { message = "Unable to link the PayPal order." });
            }

            _txLogger.LogSuccess(txId, "Payment.PayPal", "CreateOrder",
                new { checkout.OrderId, PayPalOrderId = initiation.ProviderTransactionId });

            return Ok(new
            {
                id = initiation.ProviderTransactionId,
                orderId = checkout.OrderId,
                status = initiation.Status,
                amount = checkout.TotalAmount,
                currency = _paypalOptions.Currency,
                transactionId = txId
            });
        }
        catch (BusinessException ex)
        {
            if (localOrderId.HasValue)
            {
                await _orderService.CancelOrderAsync(localOrderId.Value);
            }

            _logger.LogWarning(ex,
                "PayPal order rejected by business rule for product {ProductId}",
                request.ProductId);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            if (localOrderId.HasValue)
            {
                await _orderService.CancelOrderAsync(localOrderId.Value);
            }

            _txLogger.LogInterModuleError(txId, "Payment.PayPal", "PayPal API", "CreateOrder", ex);
            _logger.LogError(ex, "Failed to create PayPal order for product {ProductId}", request.ProductId);
            return StatusCode(502, new { message = "Không thể tạo thanh toán PayPal. Vui lòng thử lại." });
        }
    }

    // >>> MỚI: tạo đơn PayPal cho NHIỀU sản phẩm (checkout từ giỏ hàng)
    [HttpPost("create-cart-order")]
    public async Task<IActionResult> CreateCartOrder(
        [FromBody] PayPalCartCreateOrderRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request?.Items == null || request.Items.Count == 0)
            return BadRequest(new { message = "Giỏ hàng đang trống." });

        var txId = _txLogger.StartTransaction("Payment.PayPal", "CreateCartOrder",
            new { ItemCount = request.Items.Count });

        try
        {
            var username = GetUsernameFromToken();
            var checkout = await _orderService.CreateCartOrderAsync(
                username,
                request.Items,
                "PayPal",
                request.AddressId,
                request.CouponCode,
                request.CarrierKey);

            if (checkout == null)
                return BadRequest(new { message = "Unable to prepare the order." });

            var gateway = _paymentGatewayFactory.Resolve("PayPal");
            var initiation = await gateway.InitiateAsync(
                new PaymentInitiationRequest(
                    checkout.TotalAmount,
                    _paypalOptions.Currency,
                    checkout.OrderId.ToString(),
                    $"Cart order #{checkout.OrderId}"),
                cancellationToken);

            if (!initiation.Success)
            {
                await _orderService.CancelOrderAsync(checkout.OrderId);
                _txLogger.LogFailure(txId, "Payment.PayPal", "CreateCartOrder",
                    new InvalidOperationException("PayPal initiation failed."), new { checkout.OrderId });
                return StatusCode(502, new { message = "Không thể tạo thanh toán PayPal. Vui lòng thử lại." });
            }

            var attached = await _orderService.AttachPayPalOrderAsync(
                username,
                checkout.OrderId,
                initiation.ProviderTransactionId);

            if (!attached)
            {
                await _orderService.CancelOrderAsync(checkout.OrderId);
                _txLogger.LogFailure(txId, "Payment.PayPal", "CreateCartOrder",
                    new InvalidOperationException("Không thể gắn PayPal order vào đơn hàng nội bộ."),
                    new { checkout.OrderId });
                return BadRequest(new { message = "Unable to link the PayPal order." });
            }

            _txLogger.LogSuccess(txId, "Payment.PayPal", "CreateCartOrder",
                new { checkout.OrderId, PayPalOrderId = initiation.ProviderTransactionId });

            return Ok(new
            {
                id = initiation.ProviderTransactionId,
                orderId = checkout.OrderId,
                status = initiation.Status,
                amount = checkout.TotalAmount,
                currency = _paypalOptions.Currency,
                transactionId = txId
            });
        }
        catch (Exception ex)
        {
            _txLogger.LogInterModuleError(txId, "Payment.PayPal", "PayPal API", "CreateCartOrder", ex);
            _logger.LogError(ex, "Failed to create PayPal cart order");
            return StatusCode(502, new { message = "Không thể tạo thanh toán PayPal. Vui lòng thử lại." });
        }
    }

    [HttpPost("capture-order")]
    public async Task<IActionResult> CaptureOrder(
        [FromBody] PayPalCaptureOrderRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.OrderId <= 0 || string.IsNullOrWhiteSpace(request.PayPalOrderId))
            return BadRequest(new { message = "OrderId and PayPalOrderId are required." });

        var txId = _txLogger.StartTransaction("Payment.PayPal", "CaptureOrder",
            new { request.OrderId, request.PayPalOrderId });

        try
        {
            var username = GetUsernameFromToken();
            var owned = await _orderService.IsPayPalOrderOwnedAsync(
                username,
                request.OrderId,
                request.PayPalOrderId);

            if (!owned)
                return NotFound(new { message = "PayPal order không tồn tại hoặc không thuộc tài khoản này." });

            var gateway = _paymentGatewayFactory.Resolve("PayPal");
            var capture = await gateway.CaptureAsync(request.PayPalOrderId, cancellationToken);

            if (!capture.Success)
            {
                await _orderService.FailPayPalPaymentAsync(
                    username,
                    request.OrderId,
                    request.PayPalOrderId,
                    capture.Status);

                _txLogger.LogFailure(txId, "Payment.PayPal", "CaptureOrder",
                    new InvalidOperationException($"Trạng thái capture: {capture.Status}"), new { request.OrderId });
                return BadRequest(new { message = "PayPal chưa hoàn tất thanh toán.", status = capture.Status });
            }

            if (capture.Amount == null ||
                string.IsNullOrWhiteSpace(capture.ProviderTransactionId) ||
                !string.Equals(capture.Currency, _paypalOptions.Currency, StringComparison.OrdinalIgnoreCase))
            {
                await _orderService.FailPayPalPaymentAsync(
                    username,
                    request.OrderId,
                    request.PayPalOrderId,
                    "INVALID_CAPTURE_RESPONSE");

                _txLogger.LogFailure(txId, "Payment.PayPal", "CaptureOrder",
                    new InvalidOperationException("Capture trả về số tiền/tiền tệ không hợp lệ."), new { request.OrderId });
                return BadRequest(new { message = "PayPal không trả về số tiền capture hợp lệ." });
            }

            var completed = await _orderService.CompletePayPalPaymentAsync(
                username,
                request.OrderId,
                request.PayPalOrderId,
                capture.ProviderTransactionId,
                capture.Amount.Value,
                capture.Currency!);

            if (completed == null)
            {
                _txLogger.LogFailure(txId, "Payment.PayPal", "CaptureOrder",
                    new InvalidOperationException("Không tìm thấy đơn hàng nội bộ."), new { request.OrderId });
                return NotFound(new { message = "Không tìm thấy đơn hàng nội bộ để cập nhật." });
            }

            _txLogger.LogSuccess(txId, "Payment.PayPal", "CaptureOrder",
                new { request.OrderId, capture.ProviderTransactionId });

            return Ok(completed);
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning(ex,
                "PayPal capture rejected by business rule for order {OrderId}",
                request.OrderId);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _txLogger.LogInterModuleError(txId, "Payment.PayPal", "PayPal API", "CaptureOrder", ex);
            _logger.LogError(ex, "Failed to capture PayPal order {PayPalOrderId}", request.PayPalOrderId);
            return StatusCode(502, new { message = "Không thể hoàn tất thanh toán PayPal. Vui lòng thử lại." });
        }
    }

    private string GetUsernameFromToken()
    {
        return User.Identity?.Name
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? throw new InvalidOperationException("User is not authenticated.");
    }
}
