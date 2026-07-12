using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Tasks.Dtos;
using TaskManagement.Application.Tasks.Interfaces;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetAll(
        [FromQuery] TaskItemStatus? status,
        CancellationToken cancellationToken)
    {
        var tasks = await _taskService.GetAllAsync(status, cancellationToken);
        return Ok(tasks);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var task = await _taskService.GetByIdAsync(id, cancellationToken);
        return Ok(task);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskResponse>> Create(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var created = await _taskService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> Update(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        var updated = await _taskService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _taskService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
