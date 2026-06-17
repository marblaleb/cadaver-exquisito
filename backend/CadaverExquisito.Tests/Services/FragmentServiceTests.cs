using CadaverExquisito.API.Application.DTOs;
using CadaverExquisito.API.Application.Interfaces;
using CadaverExquisito.API.Application.Services;
using CadaverExquisito.API.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CadaverExquisito.Tests.Services;

public class FragmentServiceTests
{
    private readonly Mock<ICadaverRepository> _cadaverRepo = new();
    private readonly Mock<IFragmentRepository> _fragmentRepo = new();
    private readonly Mock<INotificationService> _notifications = new();
    private readonly FragmentService _sut;

    public FragmentServiceTests()
    {
        _sut = new FragmentService(_cadaverRepo.Object, _fragmentRepo.Object, _notifications.Object);
    }

    [Fact]
    public async Task AddFragment_ThrowsIfUserAlreadyParticipated()
    {
        var cadaverId = Guid.NewGuid();
        var userId = "user-1";
        _cadaverRepo.Setup(r => r.GetByIdAsync(cadaverId))
            .ReturnsAsync(new Cadaver { Id = cadaverId, MaxParticipants = 3, CurrentTurn = 2 });
        _fragmentRepo.Setup(r => r.UserHasFragmentAsync(cadaverId, userId))
            .ReturnsAsync(true);

        var act = () => _sut.AddFragmentAsync(cadaverId, userId, new AddFragmentRequest("texto"));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already*");
    }

    [Fact]
    public async Task AddFragment_ThrowsIfContentExceedsWordLimit()
    {
        var cadaverId = Guid.NewGuid();
        var userId = "user-1";
        var longContent = string.Join(" ", Enumerable.Repeat("palabra", 301));

        _cadaverRepo.Setup(r => r.GetByIdAsync(cadaverId))
            .ReturnsAsync(new Cadaver { Id = cadaverId, MaxParticipants = 3, CurrentTurn = 2 });
        _fragmentRepo.Setup(r => r.UserHasFragmentAsync(cadaverId, userId))
            .ReturnsAsync(false);

        var act = () => _sut.AddFragmentAsync(cadaverId, userId, new AddFragmentRequest(longContent));

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*300*");
    }

    [Fact]
    public async Task AddFragment_MarksCompletedWhenLastParticipant()
    {
        var cadaverId = Guid.NewGuid();
        var userId = "user-3";
        var cadaver = new Cadaver { Id = cadaverId, MaxParticipants = 3, CurrentTurn = 3 };

        _cadaverRepo.Setup(r => r.GetByIdAsync(cadaverId)).ReturnsAsync(cadaver);
        _fragmentRepo.Setup(r => r.UserHasFragmentAsync(cadaverId, userId)).ReturnsAsync(false);
        _fragmentRepo.Setup(r => r.CreateAsync(It.IsAny<Fragment>()))
            .ReturnsAsync((Fragment f) => f);
        _fragmentRepo.Setup(r => r.GetParticipantUserIdsAsync(cadaverId))
            .ReturnsAsync(new List<string> { "user-1", "user-2", "user-3" });
        _notifications.Setup(n => n.SendToUsersAsync(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _notifications.Setup(n => n.SendToNonParticipantsAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        await _sut.AddFragmentAsync(cadaverId, userId, new AddFragmentRequest("último fragmento"));

        cadaver.IsCompleted.Should().BeTrue();
        _cadaverRepo.Verify(r => r.UpdateAsync(It.Is<Cadaver>(c => c.IsCompleted)), Times.Once);
    }

    [Fact]
    public async Task GetLastFragment_ReturnsFragmentAtPreviousTurn()
    {
        var cadaverId = Guid.NewGuid();
        var cadaver = new Cadaver { Id = cadaverId, CurrentTurn = 3 };
        var fragment = new Fragment { Id = Guid.NewGuid(), Content = "texto previo", SequenceOrder = 2 };

        _cadaverRepo.Setup(r => r.GetByIdAsync(cadaverId)).ReturnsAsync(cadaver);
        _fragmentRepo.Setup(r => r.GetBySequenceOrderAsync(cadaverId, 2)).ReturnsAsync(fragment);

        var result = await _sut.GetLastFragmentAsync(cadaverId);

        result!.Content.Should().Be("texto previo");
        result.SequenceOrder.Should().Be(2);
    }

    [Fact]
    public async Task GetLastFragment_ReturnsNullOnFirstTurn()
    {
        var cadaverId = Guid.NewGuid();
        _cadaverRepo.Setup(r => r.GetByIdAsync(cadaverId))
            .ReturnsAsync(new Cadaver { Id = cadaverId, CurrentTurn = 1 });

        var result = await _sut.GetLastFragmentAsync(cadaverId);

        result.Should().BeNull();
    }
}
