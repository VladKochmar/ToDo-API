namespace ToDoApi.Models.DTOs;

public interface ITaskService
{
  public Task<TaskResponse?> GetById(Guid id);
  public Task<IReadOnlyList<TaskResponse>> GetAll();
  public Task<TaskResponse> Create(CreateTaskRequest request);
  public Task<bool> Update(Guid id, UpdateTaskRequest request);
  public Task<bool> Delete(Guid id);
}