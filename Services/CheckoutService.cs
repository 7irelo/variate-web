using Microsoft.EntityFrameworkCore;
using variate.Data;
using variate.Models;

namespace variate.Services
{
    public enum CheckoutOutcome { Paid, AlreadyPaid, NotPaid, NotFound }

    public class CheckoutService
    {
        public const string PendingPayment = "PendingPayment";
        public const string Paid = "Paid";

        private readonly ApplicationDbContext _db;
        private readonly PaystackClient _paystack;
        private readonly PaystackOptions _options;
        private readonly ILogger<CheckoutService> _logger;

        public CheckoutService(ApplicationDbContext db, PaystackClient paystack, PaystackOptions options, ILogger<CheckoutService> logger)
        {
            _db = db;
            _paystack = paystack;
            _options = options;
            _logger = logger;
        }

        // Snapshots the active cart into a pending order at current prices and
        // returns the Paystack page to send the customer to. Returns null for an
        // empty cart.
        public async Task<string?> StartAsync(ApplicationUser user, string callbackUrl, CancellationToken ct)
        {
            var cart = await _db.Carts
                .Include(c => c.CartItems).ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.ApplicationUserId == user.Id && c.Status == "Active", ct);
            if (cart == null || cart.CartItems.Count == 0) return null;

            var order = new Order { ApplicationUserId = user.Id, Status = PendingPayment, OrderDateTime = DateTime.Now };
            foreach (var item in cart.CartItems.Where(ci => ci.Product != null))
            {
                order.OrderItems.Add(new OrderItem { ProductId = item.ProductId, Quantity = item.Quantity, UnitPrice = item.Product!.EffectivePrice });
            }
            _db.Orders.Add(order);
            await _db.SaveChangesAsync(ct);

            // The order id travels in the reference so the callback and webhook
            // can find the order without another column; the random suffix keeps
            // each payment attempt unique on Paystack's side.
            var reference = $"VAR-{order.Id}-{Guid.NewGuid():N}";
            var url = await _paystack.InitializeAsync(user.Email!, ToMinorUnits(order.TotalCost), reference, callbackUrl, ct);
            _logger.LogInformation("Started Paystack checkout for order {OrderId}.", order.Id);
            return url;
        }

        // Never trusts the redirect or webhook payload: the transaction is
        // re-fetched from Paystack and must match the order's amount, currency
        // and (for a browser callback) owner before the order is marked paid.
        public async Task<(CheckoutOutcome Outcome, int OrderId)> ConfirmAsync(string reference, string? userId, CancellationToken ct)
        {
            if (!TryParseOrderId(reference, out var orderId)) return (CheckoutOutcome.NotFound, 0);

            var order = await _db.Orders.Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && (userId == null || o.ApplicationUserId == userId), ct);
            if (order == null) return (CheckoutOutcome.NotFound, 0);
            if (order.Status == Paid) return (CheckoutOutcome.AlreadyPaid, orderId);

            var transaction = await _paystack.VerifyAsync(reference, ct);
            var amount = ToMinorUnits(order.TotalCost);
            if (transaction.Status != "success" || transaction.Reference != reference
                || transaction.Amount != amount || !string.Equals(transaction.Currency, _options.Currency, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Paystack transaction for order {OrderId} not accepted: status {Status}, amount {Amount} {Currency}.",
                    orderId, transaction.Status, transaction.Amount, transaction.Currency);
                return (CheckoutOutcome.NotPaid, orderId);
            }

            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            // Conditional update so a callback and webhook racing each other
            // record the payment once.
            var updated = await _db.Orders
                .Where(o => o.Id == orderId && o.Status == PendingPayment)
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, Paid), ct);
            if (updated == 0) return (CheckoutOutcome.AlreadyPaid, orderId);

            _db.Payments.Add(new Payment
            {
                OrderId = orderId,
                PaymentMethod = "Paystack",
                Amount = order.TotalCost,
                Status = "Completed",
                PaymentDateTime = DateTime.Now
            });
            await _db.Carts
                .Where(c => c.ApplicationUserId == order.ApplicationUserId && c.Status == "Active")
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.Status, "CheckedOut"), ct);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            _logger.LogInformation("Order {OrderId} paid via Paystack.", orderId);
            return (CheckoutOutcome.Paid, orderId);
        }

        private static long ToMinorUnits(decimal amount) => (long)decimal.Round(amount * 100m, MidpointRounding.AwayFromZero);

        private static bool TryParseOrderId(string reference, out int orderId)
        {
            orderId = 0;
            var parts = reference.Split('-');
            return parts.Length == 3 && parts[0] == "VAR" && int.TryParse(parts[1], out orderId);
        }
    }
}
