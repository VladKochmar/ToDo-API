using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoApi.Services;


namespace ToDoApi.Controllers;

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