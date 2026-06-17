using CadaverExquisito.API.Application.DTOs;
using CadaverExquisito.API.Application.Interfaces;
using CadaverExquisito.API.Application.Services;
using CadaverExquisito.API.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CadaverExquisito.Tests.Services;

public class CadaverServiceTests
{
    private readonly Mock<ICadaverRepository> _cadaverRepo = new();
    private readonly Mock<IFragmentRepository> _fragmentRepo = new();
    private readonly CadaverService _sut;

    public CadaverServiceTests()
    {
        _sut = new CadaverService(_cadaverRepo.Object, _fragmentRepo.Object);
    }

    [Fact]
    public async Task GetAvailableAsync_ReturnsMappedDtos()
    {
        var userId = "user-1";
        var cadaver = new Cadaver
        {
            Id = Guid.NewGuid(),
            Title = "Historia del 18 may 2026",
            MaxParticipants = 3,
            CurrentTurn = 1,
            CreatedAt = new DateTime(2026, 5, 18)
        };
        _cadaverRepo.Setup(r => r.GetAvailableForUserAsync(userId))
            .ReturnsAsync(new List<Cadaver> { cadaver });

        var result = await _sut.GetAvailableAsync(userId);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(cadaver.Id);
        result[0].Title.Should().Be(cadaver.Title);
    }

    [Fact]
    public async Task CreateAsync_SetsTitleFromCreatedAt()
    {
        var userId = "user-1";
        var request = new CreateCadaverRequest(3, "Había una vez...");
        _cadaverRepo.Setup(r => r.CreateAsync(It.IsAny<Cadaver>()))
            .ReturnsAsync((Cadaver c) => c);
        _fragmentRepo.Setup(r => r.CreateAsync(It.IsAny<Fragment>()))
            .ReturnsAsync((Fragment f) => f);

        var result = await _sut.CreateAsync(userId, request);

        result.Title.Should().StartWith("Historia del");
        result.MaxParticipants.Should().Be(3);
    }
}
