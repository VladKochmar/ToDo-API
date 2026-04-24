namespace ToDoApi.Models.DTOs;

public interface ITaskService
{
  public Task<TaskResponse> GetById(Guid id);
  public Task<IReadOnlyList<TaskResponse>> GetAll();
  public Task<TaskResponse> Create(CreateTaskRequest request);
  public Task Update(Guid id, UpdateTaskRequest request);
  public Task Delete(Guid id);
}