using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Todo.Api.Data;
using Todo.Api.Models.DTOs;
using Todo.Api.Models.Entities;
using Todo.Api.Services;
using Todo.Api.Tenancy;
using Todo.Api.Tests.Factories;

namespace Todo.Api.Tests.Tenancy;

public class MultiTenantIsolationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _httpClient;
  private readonly CustomWebApplicationFactory<Program> _factory;

  public MultiTenantIsolationTests(CustomWebApplicationFactory<Program> factory)
  {
    _factory = factory;
    _httpClient = factory.CreateClient();
  }

  [Fact]
  public async Task GetCategories_ShouldReturnOnlyBelongingClientCategories_WhenQueriedWithClientContext()
  {
    // Arrange
    ClientResponse bmw = await CreateClient("BMW");
    ClientResponse audi = await CreateClient("Audi");

    string bmwSub = "auth0|test-bmw";
    string audiSub = "auth0|test-audi";

    await CreateUser(bmw.Id, bmwSub, "bmw@test.com", "BMW User");
    await CreateUser(audi.Id, audiSub, "audi@test.com", "Audi User");

    await CreateCategory(bmwSub, bmw.Id, "BMW Category");
    await CreateCategory(audiSub, audi.Id, "Audi Category");

    // Act
    List<CategoryResponse>? bmwCategories = await GetCategories(bmwSub, bmw.Id);

    List<CategoryResponse>? audiCategories = await GetCategories(audiSub, audi.Id);

    // Asserts
    bmwCategories.Should().NotBeNull();
    bmwCategories.Should().HaveCount(1);
    bmwCategories[0].Title.Should().Be("BMW Category");
    bmwCategories.Should().NotContain(c => c.Title == "Audi Category");

    audiCategories.Should().NotBeNull();
    audiCategories.Should().HaveCount(1);
    audiCategories[0].Title.Should().Be("Audi Category");
    audiCategories.Should().NotContain(c => c.Title == "BMW Category");
  }

  [Fact]
  public async Task GetCategories_ShouldPreventDataLeak_WhenClientFilterIsForgotten()
  {
    // Arrange
    ClientResponse bmw = await CreateClient("BMW");
    ClientResponse audi = await CreateClient("Audi");

    string bmwSub = "auth0|test-bmw";
    string audiSub = "auth0|test-audi";

    await CreateUser(bmw.Id, bmwSub, "bmw@test.com", "BMW User");
    await CreateUser(audi.Id, audiSub, "audi@test.com", "Audi User");

    await CreateCategory(bmwSub, bmw.Id, "BMW Category");
    await CreateCategory(audiSub, audi.Id, "Audi Category");

    Client? bmwClient = await GetClient(bmw.Id);

    using IServiceScope scope = _factory.Services.CreateScope();
    TenantContext tenantContext = scope.ServiceProvider.GetRequiredService<TenantContext>();

    tenantContext.CurrentTenant = new TenantInfo
    {
      ClientId = bmwClient!.Id,
      ConnectionString = $"Host=localhost;Port=5432;Database={bmwClient.DbName};Username={bmwClient.DbUser};Password={bmwClient.DbPassword}"
    };

    AppDbContext appContext = scope.ServiceProvider
      .GetRequiredService<AppDbContext>();

    // Act
    List<Category> categories = await appContext.Categories.ToListAsync();

    // Assert
    categories.Should().NotBeNull();
    categories.Should().HaveCount(1);
    categories[0].Title.Should().Be("BMW Category");
    categories.Should().NotContain(c => c.Title == "Audi Category");
  }

  private async Task<Client?> GetClient(Guid clientId)
  {
    using IServiceScope scope = _factory.Services.CreateScope();
    GlobalDbContext globalDb = scope.ServiceProvider.GetRequiredService<GlobalDbContext>();

    Client? client = await globalDb.Clients.FirstOrDefaultAsync(c => c.Id == clientId);

    return client;
  }

  private async Task<ClientResponse> CreateClient(string name)
  {
    IServiceScope scope = _factory.Services.CreateScope();
    IClientService clientService = scope.ServiceProvider.GetRequiredService<IClientService>();

    ClientRequest request = new(name);
    ClientResponse response = await clientService.Create(request);

    return response;
  }

  private string GenerateFakeIdToken(string sub, string email, string name)
  {
    List<Claim> claims = [
      new Claim(JwtRegisteredClaimNames.Sub, sub),
      new Claim("email", email),
      new Claim("name", name)
    ];

    JwtSecurityToken token = new(
      issuer: "https://test-auth0.com",
      audience: "test-audience",
      claims: claims
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  private async Task CreateUser(Guid clientId, string sub, string email, string name)
  {
    string fakeIdToken = GenerateFakeIdToken(sub, email, name);

    _httpClient.DefaultRequestHeaders.Remove("x-test-authId");
    _httpClient.DefaultRequestHeaders.Add("x-test-authId", sub);

    _httpClient.DefaultRequestHeaders.Remove("x-id-token");
    _httpClient.DefaultRequestHeaders.Add("x-id-token", fakeIdToken);

    HttpResponseMessage response = await _httpClient.PostAsync($"/api/v1/clients/{clientId}/users", null);

    response.EnsureSuccessStatusCode();
  }

  private async Task<CategoryResponse?> CreateCategory(string sub, Guid clientId, string title)
  {
    _httpClient.DefaultRequestHeaders.Remove("x-test-authId");
    _httpClient.DefaultRequestHeaders.Add("x-test-authId", sub);

    CategoryRequest request = new(title);

    HttpResponseMessage response = await _httpClient
      .PostAsJsonAsync($"/api/v1/clients/{clientId}/categories", request);

    CategoryResponse? created = await response.Content
      .ReadFromJsonAsync<CategoryResponse>();

    return created;
  }

  private async Task<List<CategoryResponse>?> GetCategories(string sub, Guid clientId)
  {
    _httpClient.DefaultRequestHeaders.Remove("x-test-authId");
    _httpClient.DefaultRequestHeaders.Add("x-test-authId", sub);
    
    HttpResponseMessage response = await _httpClient
      .GetAsync($"/api/v1/clients/{clientId}/categories");

    List<CategoryResponse>? categories = await response.Content
      .ReadFromJsonAsync<List<CategoryResponse>>();

    return categories;
  }
}