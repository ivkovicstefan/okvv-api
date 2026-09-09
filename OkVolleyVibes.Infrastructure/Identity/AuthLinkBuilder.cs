using Microsoft.Extensions.Options;
using OkVolleyVibes.Application.Common.Abstractions;
using OkVolleyVibes.Infrastructure.Configuration;

namespace OkVolleyVibes.Infrastructure.Identity;

internal sealed class AuthLinkBuilder(IOptions<AppUrls> urls) : IAuthLinkBuilder
{
    private readonly string _apiBaseUrl = urls.Value.PublicBaseUrl.TrimEnd('/');

    public string EmailConfirmationLink(Guid userId, string token)
        => $"{_apiBaseUrl}/api/auth/verify-email?userId={userId}&token={Uri.EscapeDataString(token)}";
}
