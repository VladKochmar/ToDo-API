using Microsoft.AspNetCore.Mvc;
using Todo.Api.Authorization;

namespace Todo.Api.Controllers;

[ApiController]
[Route("api/v1/clients")]
public class ClientsController : ControllerBase
{
  [HttpGet]
  [AdminOnly]
  public ActionResult<string> GetHello()
  {
    return Ok(new { message = "Hello World!" });
  }
}