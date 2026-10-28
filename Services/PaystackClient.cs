using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace variate.Services
{
    public class PaystackOptions
    {
        public const string SectionName = "Paystack";

        public string SecretKey { get; set; } = string.Empty;
        public string Currency { get; set; } = "ZAR";
        public string BaseUrl { get; set; } = "https://api.paystack.co/";

        public bool IsConfigured => SecretKey.StartsWith("sk_", StringComparison.Ordinal);
    }

    public record PaystackTransaction(string Status, string Reference, long Amount, string Currency);

    public class PaystackException : Exception
    {
        public PaystackException(string message) : base(message) { }
    }

    // Thin client over the two Paystack calls checkout needs. Amounts are in the
    // currency's minor unit (cents for ZAR).
    public class PaystackClient
    {
        private readonly HttpClient _http;
        private readonly PaystackOptions _options;

        public PaystackClient(HttpClient http, PaystackOptions options)
        {
            _http = http;
            _options = options;
            _http.BaseAddress = new Uri(options.BaseUrl);
            _http.Timeout = TimeSpan.FromSeconds(15);
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.SecretKey);
        }

        public async Task<string> InitializeAsync(string email, long amount, string reference, string callbackUrl, CancellationToken ct)
        {
            var response = await _http.PostAsJsonAsync("transaction/initialize", new
            {
                email,
                amount,
                currency = _options.Currency,
                reference,
                callback_url = callbackUrl
            }, ct);
            var body = await ReadAsync<InitializeData>(response, ct);
            return body.AuthorizationUrl ?? throw new PaystackException("Paystack returned no authorization URL.");
        }

        public async Task<PaystackTransaction> VerifyAsync(string reference, CancellationToken ct)
        {
            var response = await _http.GetAsync($"transaction/verify/{Uri.EscapeDataString(reference)}", ct);
            var data = await ReadAsync<VerifyData>(response, ct);
            return new PaystackTransaction(data.Status ?? "", data.Reference ?? "", data.Amount, data.Currency ?? "");
        }

        // Paystack signs webhook bodies with HMAC-SHA512 keyed on the secret key.
        public bool IsValidSignature(byte[] body, string? signature)
        {
            if (string.IsNullOrEmpty(signature) || !_options.IsConfigured) return false;
            var expected = HMACSHA512.HashData(Encoding.UTF8.GetBytes(_options.SecretKey), body);
            byte[] actual;
            try
            {
                actual = Convert.FromHexString(signature);
            }
            catch (FormatException)
            {
                return false;
            }
            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }

        private static async Task<T> ReadAsync<T>(HttpResponseMessage response, CancellationToken ct)
        {
            var envelope = await response.Content.ReadFromJsonAsync<Envelope<T>>(cancellationToken: ct);
            if (!response.IsSuccessStatusCode || envelope is not { Status: true, Data: not null })
            {
                throw new PaystackException($"Paystack request failed ({(int)response.StatusCode}): {envelope?.Message}");
            }
            return envelope.Data;
        }

        private record Envelope<T>(
            [property: JsonPropertyName("status")] bool Status,
            [property: JsonPropertyName("message")] string? Message,
            [property: JsonPropertyName("data")] T? Data);

        private record InitializeData(
            [property: JsonPropertyName("authorization_url")] string? AuthorizationUrl);

        private record VerifyData(
            [property: JsonPropertyName("status")] string? Status,
            [property: JsonPropertyName("reference")] string? Reference,
            [property: JsonPropertyName("amount")] long Amount,
            [property: JsonPropertyName("currency")] string? Currency);
    }
}
