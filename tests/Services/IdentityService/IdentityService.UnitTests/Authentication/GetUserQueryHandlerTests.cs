using AutoFixture;
using FluentAssertions;
using IdentityService.Application.Authentication.Queries;
using IdentityService.Application.Authentication.Handlers;
using IdentityService.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IdentityService.UnitTests.Authentication;

public class GetUserQueryHandlerTests
{
    private readonly Mock<ILogger<GetUserQueryHandler>> _mockLogger;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly GetUserQueryHandler _handler;
    private readonly Fixture _fixture;

    public GetUserQueryHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GetUserQueryHandler>>();
        _mockUserRepository = new Mock<IUserRepository>();
        _handler = new GetUserQueryHandler(_mockUserRepository.Object, _mockLogger.Object);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task Handle_Should_Return_Null_For_NonExistent_User()
    {
        // Arrange
        var query = new GetUserQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("11111111-1111-1111-1111-111111111111")]
    public async Task Handle_Should_Complete_For_Different_UserIds(string userIdString)
    {
        // Arrange
        var userId = Guid.Parse(userIdString);
        var query = new GetUserQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert - Should complete without exception
        result.Should().BeNull(); // Current stub implementation returns null
    }
}