using FluentAssertions;
using Moq;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Tasks.Dtos;
using TaskManagement.Application.Tasks.Interfaces;
using TaskManagement.Application.Tasks.Services;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.UnitTests.Tasks;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _repository = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Guid _userId = Guid.NewGuid();

    private TaskService CreateSut()
    {
        _currentUser.SetupGet(x => x.UserId).Returns(_userId);
        return new TaskService(_repository.Object, _currentUser.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task CreateAsync_persists_task_owned_by_current_user()
    {
        var sut = CreateSut();
        var request = new CreateTaskRequest(
            "Write report", "Q3 summary", DateTime.UtcNow.AddDays(3), TaskItemStatus.InProgress);

        var result = await sut.CreateAsync(request);

        result.Title.Should().Be("Write report");
        result.Description.Should().Be("Q3 summary");
        result.Status.Should().Be(TaskItemStatus.InProgress);
        result.Id.Should().NotBeEmpty();

        _repository.Verify(r => r.Add(
            It.Is<TaskItem>(t => t.UserId == _userId && t.Title == "Write report")), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_returns_current_users_tasks_mapped_to_responses()
    {
        var sut = CreateSut();
        var tasks = new List<TaskItem>
        {
            new("A", null, DateTime.UtcNow.AddDays(1), _userId),
            new("B", null, DateTime.UtcNow.AddDays(2), _userId),
        };
        _repository
            .Setup(r => r.GetByUserAsync(_userId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        var result = await sut.GetAllAsync(null);

        result.Should().HaveCount(2);
        result.Select(r => r.Title).Should().Equal("A", "B");
    }

    [Fact]
    public async Task GetAllAsync_passes_status_filter_to_repository_scoped_to_current_user()
    {
        var sut = CreateSut();
        _repository
            .Setup(r => r.GetByUserAsync(_userId, TaskItemStatus.Completed, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaskItem>());

        await sut.GetAllAsync(TaskItemStatus.Completed);

        _repository.Verify(
            r => r.GetByUserAsync(_userId, TaskItemStatus.Completed, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // A task the current user owns.
    private TaskItem OwnedTask() =>
        new("Owned", "desc", DateTime.UtcNow.AddDays(1), _userId);

    // A task owned by somebody else.
    private static TaskItem OtherUsersTask() =>
        new("Theirs", "desc", DateTime.UtcNow.AddDays(1), Guid.NewGuid());

    [Fact]
    public async Task GetByIdAsync_returns_task_when_owned_by_current_user()
    {
        var sut = CreateSut();
        var task = OwnedTask();
        _repository.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(task);

        var result = await sut.GetByIdAsync(task.Id);

        result.Id.Should().Be(task.Id);
        result.Title.Should().Be("Owned");
    }

    [Fact]
    public async Task GetByIdAsync_throws_NotFound_when_task_is_missing()
    {
        var sut = CreateSut();
        _repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((TaskItem?)null);

        Func<Task> act = () => sut.GetByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_throws_NotFound_when_task_belongs_to_another_user()
    {
        var sut = CreateSut();
        var task = OtherUsersTask();
        _repository.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(task);

        Func<Task> act = () => sut.GetByIdAsync(task.Id);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_applies_changes_and_persists_when_owned()
    {
        var sut = CreateSut();
        var task = OwnedTask();
        _repository.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(task);
        var request = new UpdateTaskRequest(
            "Updated title", "new desc", DateTime.UtcNow.AddDays(5), TaskItemStatus.Completed);

        var result = await sut.UpdateAsync(task.Id, request);

        result.Title.Should().Be("Updated title");
        result.Status.Should().Be(TaskItemStatus.Completed);
        task.Title.Should().Be("Updated title");
        _repository.Verify(r => r.Update(task), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_throws_NotFound_when_task_belongs_to_another_user()
    {
        var sut = CreateSut();
        var task = OtherUsersTask();
        _repository.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(task);
        var request = new UpdateTaskRequest("x", null, DateTime.UtcNow.AddDays(1), TaskItemStatus.Pending);

        Func<Task> act = () => sut.UpdateAsync(task.Id, request);

        await act.Should().ThrowAsync<NotFoundException>();
        _repository.Verify(r => r.Update(It.IsAny<TaskItem>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_removes_task_when_owned()
    {
        var sut = CreateSut();
        var task = OwnedTask();
        _repository.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(task);

        await sut.DeleteAsync(task.Id);

        _repository.Verify(r => r.Remove(task), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_throws_NotFound_when_task_belongs_to_another_user()
    {
        var sut = CreateSut();
        var task = OtherUsersTask();
        _repository.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(task);

        Func<Task> act = () => sut.DeleteAsync(task.Id);

        await act.Should().ThrowAsync<NotFoundException>();
        _repository.Verify(r => r.Remove(It.IsAny<TaskItem>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
