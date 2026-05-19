using Microsoft.EntityFrameworkCore;
using Todo.Api.Data;
using Todo.Api.Models.Entities;
using Todo.Api.Tenancy;

namespace Todo.Api.Middlewares;

public class TenantResolutionMiddleware(RequestDelegate next)
{
  public async Task InvokeAsync
  (
    HttpContext httpContext,
    GlobalDbContext globalDb,
    TenantContext tenantContext
  )
  {
    
    object? routeValue = httpContext.GetRouteValue("clientId");
    
    if (routeValue is null)
    {
      await next(httpContext);
      return;
    }

    if(!Guid.TryParse(routeValue.ToString(), out Guid clientId))
    {
      httpContext.Response.StatusCode = 400;
      await httpContext.Response.WriteAsync("Invalid clientId.");
      return;
    }

    Client? client = globalDb.Clients
      .AsNoTracking()
      .FirstOrDefault(c => c.Id == clientId);

    if (client is null)
    {
      httpContext.Response.StatusCode = 404;
      await httpContext.Response.WriteAsync("Cleint not found.");
      return;
    }

    string connectionString = $"Host=localhost;Port=5432;Database={client.DbName};Username={client.DbUser};Password={client.DbPassword}";

    tenantContext.CurrentTenant = new TenantInfo
    {
      ClientId = clientId,
      ConnectionString = connectionString
    };

    await next(httpContext);
  }
}