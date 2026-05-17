using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Todo.Api.Authorization;
using Todo.Api.Services;
using Todo.Api.Models.DTOs;

namespace Todo.Api.Controllers;

[Authorize]
[AdminOnly]
[ApiController]
[Route("api/v1/clients")]
public class ClientsController(
  IClientService service, 
  IValidator<ClientRequest> validator
) : ControllerBase
{
  [HttpGet]
  public async Task<ActionResult<IReadOnlyList<ClientResponse>>> GetClients()
  {
    IReadOnlyList<ClientResponse> clients = await service.GetAll();
    return Ok(clients);
  }

  [HttpPost]
  public async Task<ActionResult<ClientResponse>> CreateClient(ClientRequest request)
  {
    await validator.ValidateAndThrowAsync(request);
    ClientResponse response = await service.Create(request);
    return Created(string.Empty, response);
  }

  [HttpPut("{id:guid}")]
  public async Task<ActionResult> UpdateClient(Guid id, ClientRequest request)
  {
    await validator.ValidateAndThrowAsync(request);
    await service.Update(id, request);
    return NoContent();
  }

  [HttpDelete("{id:guid}")]
  public async Task<ActionResult> DeleteClient(Guid id)
  {
    await service.Delete(id);
    return NoContent();
  }
}