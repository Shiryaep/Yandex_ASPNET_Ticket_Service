using Application.DTO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Presentation;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace E2ETests;

[Collection("E2EDatabaseCollection")]
public class E2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly DatabaseFixture _fixture;

    public E2ETests(WebApplicationFactory<Program> factory, DatabaseFixture fixture)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("E2ETests");
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = fixture.ConnectionString
                });
            });
        });
        _fixture = fixture;
    }

    #region Auth Endpoint

    [Fact]
    public async Task AuthController_RegisterAndLoginMultipleTimes_RegisterThrowsConflict()
    {
        await _fixture.ResetDatabaseAsync();

        var client = _factory.CreateClient();

        UserAuthDto userAuthDto = new UserAuthDto()
        {
            Login = "login",
            Password = "password",
            Role = Domain.UserRoles.User
        };

        // You can register only once
        var response = await client.PostAsJsonAsync("/api/auth/register", userAuthDto);
        response.EnsureSuccessStatusCode();

        response = await client.PostAsJsonAsync("/api/auth/register", userAuthDto);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        // But you can log in multiple times
        response = await client.PostAsJsonAsync("/api/auth/login", userAuthDto);
        response.EnsureSuccessStatusCode();

        response = await client.PostAsJsonAsync("/api/auth/login", userAuthDto);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AuthController_LoginWithWrongCreditals_ThrowsConflict()
    {
        await _fixture.ResetDatabaseAsync();

        var client = _factory.CreateClient();

        UserAuthDto userAuthDto = new UserAuthDto()
        {
            Login = "login",
            Password = "password",
            Role = Domain.UserRoles.User
        };

        UserAuthDto userAuthDtoLoginChanged = new UserAuthDto()
        {
            Login = "NEWlogin",
            Password = "password",
            Role = Domain.UserRoles.User
        };

        UserAuthDto userAuthDtoPasswordChanged = new UserAuthDto()
        {
            Login = "login",
            Password = "NEWpassword",
            Role = Domain.UserRoles.User
        };

        var response = await client.PostAsJsonAsync("/api/auth/register", userAuthDto);
        response.EnsureSuccessStatusCode();

        response = await client.PostAsJsonAsync("/api/auth/login", userAuthDtoLoginChanged);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        response = await client.PostAsJsonAsync("/api/auth/login", userAuthDtoPasswordChanged);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Events Endpoint

    [Fact]
    public async Task EventPost_WithoutTokenUserTokenAndAdminToken_OnlyAdminSuccessfullyPostEvent()
    {
        await _fixture.ResetDatabaseAsync();

        var client = _factory.CreateClient();

        UserAuthDto userAuthDto = new UserAuthDto()
        {
            Login = "login",
            Password = "password",
            Role = Domain.UserRoles.User
        };
        var userResponse = await client.PostAsJsonAsync("/api/auth/register", userAuthDto);
        userResponse = await client.PostAsJsonAsync("/api/auth/login", userAuthDto);
        userResponse.EnsureSuccessStatusCode();
        var userToken = await userResponse.Content.ReadAsStringAsync();

        UserAuthDto userAuthDtoAdmin = new UserAuthDto()
        {
            Login = "loginAdmin",
            Password = "password",
            Role = Domain.UserRoles.Admin
        };
        var adminResponse = await client.PostAsJsonAsync("/api/auth/register", userAuthDtoAdmin);
        adminResponse = await client.PostAsJsonAsync("/api/auth/login", userAuthDtoAdmin);
        adminResponse.EnsureSuccessStatusCode();
        var adminToken = await adminResponse.Content.ReadAsStringAsync();

        CreateEventDto createEventDto = new CreateEventDto()
        {
            Title = "Test",
            Description = "Test",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(2),
            TotalSeats = 10
        };

        // No token - Unauthorized
        using var request401 = new HttpRequestMessage(HttpMethod.Post, "/api/events");
        request401.Content = new StringContent(JsonSerializer.Serialize(createEventDto), Encoding.UTF8, "application/json");
        using var response401 = await client.SendAsync(request401);
        Assert.Equal(HttpStatusCode.Unauthorized, response401.StatusCode);

        // User token - Lack of Rights
        using var request403 = new HttpRequestMessage(HttpMethod.Post, "/api/events");
        request403.Content = new StringContent(JsonSerializer.Serialize(createEventDto), Encoding.UTF8, "application/json");
        request403.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken);
        using var response403 = await client.SendAsync(request403);
        Assert.Equal(HttpStatusCode.Forbidden, response403.StatusCode);

        // Admin token - Successfull Post
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/events");
        request.Content = new StringContent(JsonSerializer.Serialize(createEventDto), Encoding.UTF8, "application/json");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    #endregion
}
