using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Backend.Configuration;
using Microsoft.Extensions.Options;

namespace Backend.Services;

public sealed class PayPalClient : IPayPalClient
{
    private readonly HttpClient _httpClient;
    private readonly PayPalOptions _options;
    private readonly ILogger<PayPalClient> _logger;

    public PayPalClient(
        HttpClient httpClient,
        IOptions<PayPalOptions> options,
        ILogger<PayPalClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<PayPalOrderResult> CreateOrderAsync(
        decimal amount,
        string referenceId,
        string description,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            intent = "CAPTURE",
            purchase_units = new[]
            {
                new
                {
                    reference_id = referenceId,
                    custom_id = referenceId,
                    description,
                    amount = new
                    {
                        currency_code = _options.Currency,
                        value = amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
                    }
                }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/v2/checkout/orders")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessTokenAsync(cancellationToken));
        request.Headers.Add("PayPal-Request-Id", $"create-{referenceId}-{Guid.NewGuid():N}");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        EnsureSuccess(response, body, "create PayPal order");

        var result = JsonSerializer.Deserialize<PayPalOrderResponse>(body)
                     ?? throw new InvalidOperationException("PayPal returned an empty order response.");

        if (string.IsNullOrWhiteSpace(result.Id))
            throw new InvalidOperationException("PayPal did not return an order id.");

        return new PayPalOrderResult(result.Id, result.Status ?? string.Empty);
    }

    public async Task<PayPalCaptureResult> CaptureOrderAsync(
        string paypalOrderId,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/v2/checkout/orders/{Uri.EscapeDataString(paypalOrderId)}/capture")
        {
            Content = JsonContent.Create(new { })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessTokenAsync(cancellationToken));
        request.Headers.Add("PayPal-Request-Id", $"capture-{paypalOrderId}");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        EnsureSuccess(response, body, "capture PayPal order");

        var result = JsonSerializer.Deserialize<PayPalCaptureResponse>(body)
                     ?? throw new InvalidOperationException("PayPal returned an empty capture response.");

        var capture = result.PurchaseUnits?
            .FirstOrDefault()?
            .Payments?
            .Captures?
            .FirstOrDefault();

        return new PayPalCaptureResult(
            capture?.Id ?? string.Empty,
            capture?.Status ?? result.Status ?? string.Empty,
            capture?.Amount?.CurrencyCode,
            decimal.TryParse(
                capture?.Amount?.Value,
                System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture,
                out var amount) ? amount : null);
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId) ||
            string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            throw new InvalidOperationException(
                "PayPal is not configured. Set PayPal:ClientId and PayPal:ClientSecret.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/oauth2/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials"
            })
        };

        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        EnsureSuccess(response, body, "get PayPal access token");

        var token = JsonSerializer.Deserialize<PayPalTokenResponse>(body);
        if (string.IsNullOrWhiteSpace(token?.AccessToken))
            throw new InvalidOperationException("PayPal did not return an access token.");

        return token.AccessToken;
    }

    private void EnsureSuccess(
        HttpResponseMessage response,
        string body,
        string operation)
    {
        if (response.IsSuccessStatusCode)
            return;

        _logger.LogError(
            "PayPal {Operation} failed with HTTP {StatusCode}: {Body}",
            operation,
            (int)response.StatusCode,
            body);

        throw new HttpRequestException(
            $"PayPal failed to {operation}. HTTP {(int)response.StatusCode}.");
    }

    private sealed class PayPalTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }

    private sealed class PayPalOrderResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }

    private sealed class PayPalCaptureResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }
        [JsonPropertyName("purchase_units")]
        public List<PayPalPurchaseUnit>? PurchaseUnits { get; set; }
    }

    private sealed class PayPalPurchaseUnit
    {
        [JsonPropertyName("payments")]
        public PayPalPayments? Payments { get; set; }
    }

    private sealed class PayPalPayments
    {
        [JsonPropertyName("captures")]
        public List<PayPalCapture>? Captures { get; set; }
    }

    private sealed class PayPalCapture
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        [JsonPropertyName("status")]
        public string? Status { get; set; }
        [JsonPropertyName("amount")]
        public PayPalAmount? Amount { get; set; }
    }

    private sealed class PayPalAmount
    {
        [JsonPropertyName("currency_code")]
        public string? CurrencyCode { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }
}
