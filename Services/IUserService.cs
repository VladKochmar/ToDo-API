using ToDoApi.Models.DTOs;

namespace ToDoApi.Services;

public interface IUserService
{
  Task SyncUser(string idToken);
}