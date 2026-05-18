using Microsoft.EntityFrameworkCore;

using Todo.Api.Data;
using Todo.Api.Exceptions;
using Todo.Api.Models.DTOs;
using Todo.Api.Models.Entities;

namespace Todo.Api.Services;

public class ClientService
(
  GlobalDbContext context, 
  ITenantProvisioningService provisioning,
  IDbCredentialsGenerator credentialsGenerator
) : IClientService
{
  public async Task<ClientResponse> Create(ClientRequest request)
  {
    string trimmedName = request.Name.Trim();

    DbCredentials credentials = credentialsGenerator.Generate();

    Client client = new()
    {
      Id = Guid.CreateVersion7(),
      Name = trimmedName,
      DbName = credentials.DbName,
      DbUser = credentials.DbUser,
      DbPassword = credentials.DbPassword
    };

    await provisioning.ProvisionTenant(client);

    context.Clients.Add(client);
    await context.SaveChangesAsync();

    return new ClientResponse(client.Id, client.Name);
  }

  public async Task Delete(Guid id)
  {
    Client client = await GetClientOrThrow(id);

    context.Clients.Remove(client);
    await context.SaveChangesAsync();
  }

  public async Task<IReadOnlyList<ClientResponse>> GetAll()
  {
    List<ClientResponse> clients = await context.Clients
      .AsNoTracking()
      .Select(c => new ClientResponse(c.Id, c.Name))
      .ToListAsync();

    return clients;
  }

  public async Task Update(Guid id, ClientRequest request)
  {
    Client client = await GetClientOrThrow(id);

    client.Name = request.Name.Trim();
    await context.SaveChangesAsync();
  }

  private async Task<Client> GetClientOrThrow(Guid id)
  {
    Client? client = await context.Clients
      .FirstOrDefaultAsync(c => c.Id == id);

    if (client is null)
      throw new NotFoundException(id, "Client");

    return client;
  }
}