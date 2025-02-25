namespace Variate.PaymentService.Gateways;

public interface IPaymentGateway
{
    string Provider { get; }
    Task<GatewayChargeResult> ChargeAsync(GatewayChargeRequest request, CancellationToken cancellationToken);
}

public sealed record GatewayChargeRequest(
    Guid PaymentId,
    Guid OrderId,
    string UserId,
    decimal Amount,
    string Currency,
    string? PaymentMethodToken);

public sealed record GatewayChargeResult(
    bool Success,
    string Status,
    string? ExternalReference,
    string? FailureReason);
