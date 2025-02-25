namespace Variate.PaymentService.Gateways;

public sealed class FakeGateway : IPaymentGateway
{
    public string Provider => "fake";

    public Task<GatewayChargeResult> ChargeAsync(GatewayChargeRequest request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            return Task.FromResult(new GatewayChargeResult(false, "Failed", null, "Amount must be greater than zero."));
        }

        if (!string.IsNullOrWhiteSpace(request.PaymentMethodToken) &&
            request.PaymentMethodToken.Contains("decline", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new GatewayChargeResult(false, "Declined", null, "Card was declined by the provider."));
        }

        var reference = $"fake_{request.PaymentId:N}_{DateTime.UtcNow:yyyyMMddHHmmss}";
        return Task.FromResult(new GatewayChargeResult(true, "Completed", reference, null));
    }
}
