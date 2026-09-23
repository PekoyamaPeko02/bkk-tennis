using BkkTennis.Api.Services;
using Microsoft.Extensions.Options;

namespace BkkTennis.Api.Features.Payments;

public record PaymentIntent(string Provider, string ProviderRef, string Method, string QrPayload, DateTime ExpiresAt);

/// <summary>
/// Abstraction over the real gateway (Opn/Omise, GB Prime Pay, 2C2P — see architecture doc).
/// Implementations create a charge and later confirm it through <c>/api/v1/payments/webhook/{provider}</c>.
/// </summary>
public interface IPaymentProvider
{
    string Name { get; }
    Task<PaymentIntent> CreatePromptPayAsync(Guid bookingId, decimal amount, DateTime expiresAt, CancellationToken ct);
    Task<bool> RefundAsync(string providerRef, decimal amount, CancellationToken ct);
    /// <summary>Validate a webhook request (signature etc.) and extract providerRef + success flag.</summary>
    Task<WebhookResult?> ParseWebhookAsync(HttpRequest request, string rawBody, CancellationToken ct);
}

public record WebhookResult(string ProviderRef, bool Succeeded);

public class PaymentOptions
{
    public string Provider { get; set; } = "mock";
    /// <summary>PromptPay ID used by the mock provider to render a scannable (but unconfirmed) QR.</summary>
    public string MockPromptPayId { get; set; } = "0812345678";
    public string WebhookSecret { get; set; } = "dev-webhook-secret";
}

/// <summary>Local-dev provider: renders a real PromptPay QR payload; payment is confirmed via a signed webhook
/// or the Development-only simulate endpoint.</summary>
public class MockPaymentProvider(IOptions<PaymentOptions> options) : IPaymentProvider
{
    public string Name => "mock";

    public Task<PaymentIntent> CreatePromptPayAsync(Guid bookingId, decimal amount, DateTime expiresAt, CancellationToken ct)
    {
        var payload = PromptPay.Payload(options.Value.MockPromptPayId, amount);
        return Task.FromResult(new PaymentIntent(Name, $"mock_{Guid.NewGuid():N}", "promptpay", payload, expiresAt));
    }

    public Task<bool> RefundAsync(string providerRef, decimal amount, CancellationToken ct) => Task.FromResult(true);

    public Task<WebhookResult?> ParseWebhookAsync(HttpRequest request, string rawBody, CancellationToken ct)
    {
        if (request.Headers["X-Webhook-Secret"] != options.Value.WebhookSecret)
            return Task.FromResult<WebhookResult?>(null);

        var doc = System.Text.Json.JsonDocument.Parse(rawBody).RootElement;
        var providerRef = doc.GetProperty("providerRef").GetString() ?? "";
        var status = doc.GetProperty("status").GetString();
        return Task.FromResult<WebhookResult?>(new WebhookResult(providerRef, status == "succeeded"));
    }
}
