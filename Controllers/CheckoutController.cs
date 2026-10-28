using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using variate.Models;
using variate.Services;

namespace variate.Controllers
{
    [Authorize]
    [Route("checkout")]
    public class CheckoutController : Controller
    {
        private readonly CheckoutService _checkout;
        private readonly PaystackClient _paystack;
        private readonly PaystackOptions _options;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(CheckoutService checkout, PaystackClient paystack, PaystackOptions options,
            UserManager<ApplicationUser> userManager, ILogger<CheckoutController> logger)
        {
            _checkout = checkout;
            _paystack = paystack;
            _options = options;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(bool acceptTerms, CancellationToken ct)
        {
            if (!acceptTerms) return BackToCart("Please accept the terms to check out.");
            if (!_options.IsConfigured) return BackToCart("Payments aren't configured on this server yet.");

            var user = await _userManager.GetUserAsync(User);
            if (user?.Email == null) return Unauthorized();

            try
            {
                var callbackUrl = Url.Action(nameof(Callback), "Checkout", null, Request.Scheme)!;
                var paymentUrl = await _checkout.StartAsync(user, callbackUrl, ct);
                return paymentUrl == null ? BackToCart("Your cart is empty.") : Redirect(paymentUrl);
            }
            catch (Exception e) when (e is PaystackException or HttpRequestException or TaskCanceledException)
            {
                _logger.LogError(e, "Could not start Paystack checkout.");
                return BackToCart("We couldn't reach the payment provider. Please try again.");
            }
        }

        // Paystack redirects the shopper here with ?reference=... after payment.
        [HttpGet("callback")]
        public async Task<IActionResult> Callback(string? reference, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(reference)) return RedirectToAction("Index", "Cart");
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            try
            {
                var (outcome, orderId) = await _checkout.ConfirmAsync(reference, user.Id, ct);
                ViewData["OrderId"] = orderId;
                return View("Result", outcome);
            }
            catch (Exception e) when (e is PaystackException or HttpRequestException or TaskCanceledException)
            {
                _logger.LogError(e, "Could not verify Paystack payment.");
                return View("Result", CheckoutOutcome.NotPaid);
            }
        }

        // Server-to-server notification; confirms payments even when the shopper
        // closes the tab before the redirect. Configure the URL in the Paystack dashboard.
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [HttpPost("paystack/webhook")]
        [RequestSizeLimit(64 * 1024)]
        public async Task<IActionResult> Webhook(CancellationToken ct)
        {
            using var buffer = new MemoryStream();
            await Request.Body.CopyToAsync(buffer, ct);
            var body = buffer.ToArray();
            if (!_paystack.IsValidSignature(body, Request.Headers["x-paystack-signature"])) return Unauthorized();

            using var json = JsonDocument.Parse(body);
            var root = json.RootElement;
            if (root.TryGetProperty("event", out var evt) && evt.GetString() == "charge.success"
                && root.TryGetProperty("data", out var data) && data.TryGetProperty("reference", out var reference)
                && reference.GetString() is { } value)
            {
                await _checkout.ConfirmAsync(value, null, ct);
            }
            return Ok();
        }

        private IActionResult BackToCart(string message)
        {
            TempData["CheckoutError"] = message;
            return RedirectToAction("Index", "Cart");
        }
    }
}
