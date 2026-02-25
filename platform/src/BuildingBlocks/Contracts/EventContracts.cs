namespace Variate.BuildingBlocks.Contracts;

public sealed record OrderLine(
    Guid ProductId,
    string Name,
    int Quantity,
    decimal UnitPrice);

public sealed record OrderCreatedEvent(
    Guid OrderId,
    string UserId,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal Total,
    string Currency,
    DateTime CreatedAtUtc,
    IReadOnlyCollection<OrderLine> Lines);

public sealed record PaymentRequestedEvent(
    Guid PaymentId,
    Guid OrderId,
    string UserId,
    decimal Amount,
    string Currency,
    string Provider,
    DateTime RequestedAtUtc);

public sealed record PaymentCompletedEvent(
    Guid PaymentId,
    Guid OrderId,
    string UserId,
    decimal Amount,
    string Currency,
    string Provider,
    string Status,
    string? ExternalReference,
    DateTime ProcessedAtUtc);
