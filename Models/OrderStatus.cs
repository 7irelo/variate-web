namespace variate.Models;

public static class OrderStatus
{
    public static string Label(string? status) => status switch
    {
        "PendingPayment" => "Awaiting payment",
        "Paid" => "Paid",
        null or "" => "Unknown",
        _ => status
    };
}
