using Ardalis.SharedKernel;
using GoalManager.Core.GoalManagement;
using GoalManager.Core.GoalManagement.Specifications;
using GoalManager.UseCases.GoalManagement.AddGoalPeriod;
using NSubstitute;
using Xunit;

namespace GoalManager.UseCases.Tests.GoalManagement.AddGoalPeriod;

[Trait("Category", "AddGoalPeriodCommandHandler")]
public sealed class AddGoalPeriodCommandHandlerTests
{
  [Fact]
  public async Task Handle_Returns_error_when_period_already_exists()
  {
    // Arrange
    var cmd = new AddGoalPeriodCommand(TeamId: 10, UserId: 1, Year: 2025);
    var goalPeriodRepository = Substitute.For<IRepository<GoalPeriod>>();
    goalPeriodRepository.AnyAsync(Arg.Any<GoalPeriodByTeamIdAndYearSpec>(), Arg.Any<CancellationToken>())
      .Returns(true);

    var sut = CreateAddGoalPeriodCommandHandler(goalPeriodRepository);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Contains(result.Errors, e => e.Contains("already exists", StringComparison.OrdinalIgnoreCase));
    await goalPeriodRepository.DidNotReceive().AddAsync(Arg.Any<GoalPeriod>(), Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task Handle_Succeeds_and_adds_new_period()
  {
    // Arrange
    var cmd = new AddGoalPeriodCommand(TeamId: 22, UserId: 5, Year: 2030);
    var goalPeriodRepository = Substitute.For<IRepository<GoalPeriod>>();
    goalPeriodRepository.AnyAsync(Arg.Any<GoalPeriodByTeamIdAndYearSpec>(), Arg.Any<CancellationToken>())
      .Returns(false);

    goalPeriodRepository.AddAsync(Arg.Any<GoalPeriod>(), Arg.Any<CancellationToken>())
      .Returns(ci => (GoalPeriod)ci[0]!); // Return the same instance; Id will be default (0) until persistence layer assigns it

    var sut = CreateAddGoalPeriodCommandHandler(goalPeriodRepository);

    // Act
    var result = await sut.Handle(cmd, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.Empty(result.Errors);
    Assert.True(result.Value >= 0); // Id default (0) acceptable in unit test without persistence
    await goalPeriodRepository.Received(1).AnyAsync(Arg.Any<GoalPeriodByTeamIdAndYearSpec>(), Arg.Any<CancellationToken>());
    await goalPeriodRepository.Received(1).AddAsync(Arg.Is<GoalPeriod>(p => p.TeamId == cmd.TeamId && p.Year == cmd.Year), Arg.Any<CancellationToken>());
  }

  private static AddGoalPeriodCommandHandler CreateAddGoalPeriodCommandHandler(IRepository<GoalPeriod>? goalPeriodRepository = null)
  {
    return new AddGoalPeriodCommandHandler(goalPeriodRepository ?? Substitute.For<IRepository<GoalPeriod>>());
  }
}
