namespace Variate.PaymentService.Gateways;

public sealed class StripeGateway(StripeGatewayOptions options) : IPaymentGateway
{
    private readonly StripeGatewayOptions _options = options;
    public string Provider => "stripe";

    public Task<GatewayChargeResult> ChargeAsync(GatewayChargeRequest request, CancellationToken cancellationToken)
    {
        // Placeholder implementation. Replace with Stripe SDK calls if you enable live charging.
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            return Task.FromResult(new GatewayChargeResult(
                false,
                "Failed",
                null,
                "Stripe secret key is not configured."));
        }

        var reference = $"stripe_{request.PaymentId:N}";
        return Task.FromResult(new GatewayChargeResult(true, "Completed", reference, null));
    }
}

public sealed class StripeGatewayOptions
{
    public const string SectionName = "Stripe";
    public string SecretKey { get; init; } = string.Empty;
}
