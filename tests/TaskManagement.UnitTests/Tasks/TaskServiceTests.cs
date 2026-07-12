using FluentAssertions;
using Moq;
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
    private readonly Guid _userId = Guid.NewGuid();

    private TaskService CreateSut()
    {
        _currentUser.SetupGet(x => x.UserId).Returns(_userId);
        return new TaskService(_repository.Object, _currentUser.Object);
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

        _repository.Verify(r => r.AddAsync(
            It.Is<TaskItem>(t => t.UserId == _userId && t.Title == "Write report"),
            It.IsAny<CancellationToken>()), Times.Once);
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
}
