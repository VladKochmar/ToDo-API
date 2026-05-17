using Todo.Api.Exceptions;
using Todo.Api.Models.DTOs;

namespace Todo.Api.Services;

public interface IClientService
{
  /// <summary>
  /// Gets a list of all clients.
  /// </summary>
  /// <returns>
  /// A read-only list of <see cref="ClientResponse"/> objects. 
  /// If there are no clients, an empty list is returned.
  /// </returns>
  public Task<IReadOnlyList<ClientResponse>> GetAll();

  /// <summary>
  /// Creates a new client.
  /// </summary>
  /// <param name="request">An object with new data for a client (Name).</param>
  /// <returns>
  /// A <see cref="ClientResponse"/> object with the identifier and name of the created client.
  /// </returns>
  public Task<ClientResponse> Create(ClientRequest request);

  /// <summary>
  /// Updates a client by its ID.
  /// </summary>
  /// <param name="id">The unique identifier (GUID) of the client to be updated.</param>
  /// <param name="request">An object with new data for a client (Name).</param>
  /// <returns>Task</returns>
  /// <exception cref="NotFoundException">
  /// Occurs if the client with the specified <paramref name="id"/> is not found.
  /// </exception>
  public Task Update(Guid id, ClientRequest request);

  /// <summary>
  /// Deletes a client by its ID.
  /// </summary>
  /// <param name="id">The unique identifier (GUID) of the client to be deleted.</param>
  /// <returns>Task</returns>
  /// <exception cref="NotFoundException">
  /// Occurs if the client with the specified <paramref name="id"/> is not found.
  /// </exception>
  public Task Delete(Guid id);
}