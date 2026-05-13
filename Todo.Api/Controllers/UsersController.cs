using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Services;


namespace Todo.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/users")]
public class UsersController(IUserService service) : ControllerBase
{
  [HttpPost]
  public async Task<IActionResult> SyncUser()
  {    
    string idToken = Request.Headers["x-id-token"].ToString();
    await service.SyncUser(idToken);
    return Ok(new { message = "User synced successfully." });
  }
}