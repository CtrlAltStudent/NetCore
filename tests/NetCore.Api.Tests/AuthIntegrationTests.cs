using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace NetCore.Api.Tests;

/// <summary>
/// Integration tests that do not require a database (endpoint routing, auth filter).
/// Tests that call register/login need PostgreSQL; run them locally or in CI with a Postgres service.
/// </summary>
public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/v1/sales-channels");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithValidPayload_ReturnsOk()
    {
        var email = $"test-{Guid.NewGuid():N}@example.com";
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            Email = email,
            Password = "TestPassword123!",
            OrganizationName = "Test Org"
        });
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.NotEmpty(auth!.Token);
        Assert.Equal(email, auth.Email);
    }
}

internal sealed class AuthResponse
{
    public string Token { get; set; } = "";
    public string Email { get; set; } = "";
    public Guid OrganizationId { get; set; }
    public string Role { get; set; } = "";
}
