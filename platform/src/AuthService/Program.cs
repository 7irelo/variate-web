using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

builder.Services.AddSingleton(new PasswordHasher<UserAccount>());
builder.Services.AddSingleton<UserStore>();
builder.Services.AddSingleton<RefreshTokenStore>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

var auth = app.MapGroup("/api/v1/auth");

auth.MapPost("/register", (RegisterRequest request, UserStore users, PasswordHasher<UserAccount> hasher) =>
{
    var email = request.Email.Trim().ToLowerInvariant();
    if (users.Exists(email))
    {
        return Results.Conflict(new { message = "An account with this email already exists." });
    }

    var account = new UserAccount
    {
        Id = Guid.NewGuid().ToString("N"),
        Email = email,
        FirstName = request.FirstName.Trim(),
        LastName = request.LastName.Trim(),
        Roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "customer" }
    };

    account.PasswordHash = hasher.HashPassword(account, request.Password);
    users.Upsert(account);
    return Results.Created($"/api/v1/auth/users/{account.Id}", new { account.Id, account.Email });
});

auth.MapPost("/login", (LoginRequest request, UserStore users, PasswordHasher<UserAccount> hasher, RefreshTokenStore refreshTokens) =>
{
    var email = request.Email.Trim().ToLowerInvariant();
    var account = users.GetByEmail(email);
    if (account is null)
    {
        return Results.Unauthorized();
    }

    var verification = hasher.VerifyHashedPassword(account, account.PasswordHash, request.Password);
    if (verification is PasswordVerificationResult.Failed)
    {
        return Results.Unauthorized();
    }

    var accessToken = TokenIssuer.IssueJwt(account, jwtOptions);
    var refreshToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(48));
    refreshTokens.Upsert(refreshToken, account.Id, DateTime.UtcNow.AddDays(jwtOptions.RefreshTokenDays));

    return Results.Ok(new AuthTokenResponse(accessToken.Token, refreshToken, accessToken.ExpiresAtUtc));
});

auth.MapPost("/refresh", (RefreshTokenRequest request, UserStore users, RefreshTokenStore refreshTokens) =>
{
    var refresh = refreshTokens.Get(request.RefreshToken);
    if (refresh is null || refresh.ExpiresAtUtc <= DateTime.UtcNow)
    {
        return Results.Unauthorized();
    }

    var user = users.GetById(refresh.UserId);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    refreshTokens.Remove(request.RefreshToken);
    var rotatedRefresh = Convert.ToHexString(RandomNumberGenerator.GetBytes(48));
    refreshTokens.Upsert(rotatedRefresh, user.Id, DateTime.UtcNow.AddDays(jwtOptions.RefreshTokenDays));

    var accessToken = TokenIssuer.IssueJwt(user, jwtOptions);
    return Results.Ok(new AuthTokenResponse(accessToken.Token, rotatedRefresh, accessToken.ExpiresAtUtc));
});

auth.MapGet("/me", (ClaimsPrincipal user, UserStore users) =>
{
    var userId = user.FindFirstValue(JwtRegisteredClaimNames.Sub);
    if (string.IsNullOrWhiteSpace(userId))
    {
        return Results.Unauthorized();
    }

    var account = users.GetById(userId);
    if (account is null)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new
    {
        account.Id,
        account.Email,
        account.FirstName,
        account.LastName,
        account.Roles
    });
}).RequireAuthorization();

auth.MapGet("/health", () => Results.Ok(new { status = "ok", service = "auth-service" }));

app.Run();

internal sealed record RegisterRequest(string Email, string Password, string FirstName, string LastName);
internal sealed record LoginRequest(string Email, string Password);
internal sealed record RefreshTokenRequest(string RefreshToken);
internal sealed record AuthTokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc);
internal sealed record RefreshTokenRecord(string UserId, DateTime ExpiresAtUtc);

internal sealed class UserAccount
{
    public string Id { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public HashSet<string> Roles { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

internal sealed class UserStore
{
    private readonly ConcurrentDictionary<string, UserAccount> _usersById = new();
    private readonly ConcurrentDictionary<string, string> _emailToId = new(StringComparer.OrdinalIgnoreCase);

    public bool Exists(string email) => _emailToId.ContainsKey(email);

    public UserAccount? GetByEmail(string email)
    {
        return _emailToId.TryGetValue(email, out var userId) && _usersById.TryGetValue(userId, out var user)
            ? user
            : null;
    }

    public UserAccount? GetById(string id) => _usersById.TryGetValue(id, out var user) ? user : null;

    public void Upsert(UserAccount user)
    {
        _usersById[user.Id] = user;
        _emailToId[user.Email] = user.Id;
    }
}

internal sealed class RefreshTokenStore
{
    private readonly ConcurrentDictionary<string, RefreshTokenRecord> _tokens = new();

    public void Upsert(string token, string userId, DateTime expiresAtUtc)
    {
        _tokens[token] = new RefreshTokenRecord(userId, expiresAtUtc);
    }

    public RefreshTokenRecord? Get(string token)
    {
        return _tokens.TryGetValue(token, out var record) ? record : null;
    }

    public void Remove(string token)
    {
        _tokens.TryRemove(token, out _);
    }
}

internal static class TokenIssuer
{
    public static (string Token, DateTime ExpiresAtUtc) IssueJwt(UserAccount user, JwtOptions options)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(options.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName)
        };

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey));
        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}

internal sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; init; } = "variate.auth";
    public string Audience { get; init; } = "variate.services";
    public string SigningKey { get; init; } = "replace-this-in-real-deployments-with-a-long-secret";
    public int AccessTokenMinutes { get; init; } = 30;
    public int RefreshTokenDays { get; init; } = 14;
}
