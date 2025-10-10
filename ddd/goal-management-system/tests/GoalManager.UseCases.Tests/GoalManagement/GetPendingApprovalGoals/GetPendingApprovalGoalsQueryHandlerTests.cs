using GoalManager.UseCases.GoalManagement.GetPendingApprovalGoals;
using GoalManager.UseCases.Identity;
using GoalManager.UseCases.Organisation;
using GoalManager.UseCases.GoalManagement; // added for IGoalManagementQueryService
using NSubstitute;
using Xunit;

namespace GoalManager.UseCases.Tests.GoalManagement.GetPendingApprovalGoals;

[Trait("Category", "GoalManagement/GetPendingApprovalGoals")]
public sealed class GetPendingApprovalGoalsQueryHandlerTests
{
  [Fact]
  public async Task Handle_Returns_empty_list_when_no_teams()
  {
    // Arrange
    var goalMgmt = Substitute.For<IGoalManagementQueryService>();
    var org = Substitute.For<IOrganisationQueryService>();
    var id = Substitute.For<IIdentityQueryService>();

    org.GetTeamLeaderTeamIds(Arg.Any<int>()).Returns(new List<int>()); // no teams
    goalMgmt.GetPendingApprovalGoals(Arg.Any<IList<int>>()).Returns(new List<PendingApprovalGoalDto>()); // no goals
    org.GetTeamNamesAsync(Arg.Any<List<int>>()).Returns(new Dictionary<int, string>());
    id.GetUserEmails(Arg.Any<IList<int>>()).Returns(new Dictionary<int, string>());

    var sut = CreateHandler(goalMgmt, org, id);
    var query = new GetPendingApprovalGoalsQuery(TeamLeaderUserId: 500);

    // Act
    var result = await sut.Handle(query, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.Empty(result);
    await org.Received(1).GetTeamLeaderTeamIds(query.TeamLeaderUserId);
    await goalMgmt.Received(1).GetPendingApprovalGoals(Arg.Is<IList<int>>(l => l.Count == 0));
    await org.Received(1).GetTeamNamesAsync(Arg.Is<List<int>>(l => l.Count == 0));
    await id.Received(1).GetUserEmails(Arg.Is<IList<int>>(l => l.Count == 0));
  }

  [Fact]
  public async Task Handle_Populates_team_and_user_names_for_each_pending_goal()
  {
    // Arrange
    var teamIds = new List<int> { 10, 20 };
    var pending = new List<PendingApprovalGoalDto>
    {
      new() { TeamId = 10, UserId = 1, GoalId = 100, GoalSetId = 1000, GoalTitle = "G1" },
      new() { TeamId = 20, UserId = 2, GoalId = 200, GoalSetId = 2000, GoalTitle = "G2" }
    };

    var goalMgmt = Substitute.For<IGoalManagementQueryService>();
    var org = Substitute.For<IOrganisationQueryService>();
    var id = Substitute.For<IIdentityQueryService>();

    org.GetTeamLeaderTeamIds(Arg.Any<int>()).Returns(teamIds);
    goalMgmt.GetPendingApprovalGoals(teamIds).Returns(pending);
    org.GetTeamNamesAsync(teamIds).Returns(new Dictionary<int, string>
    {
      [10] = "Team-A",
      [20] = "Team-B"
    });
    id.GetUserEmails(Arg.Is<IList<int>>(u => u.Count == 2 && u.Contains(1) && u.Contains(2)))
      .Returns(new Dictionary<int, string>
      {
        [1] = "user1@example.com",
        [2] = "user2@example.com"
      });

    var sut = CreateHandler(goalMgmt, org, id);
    var query = new GetPendingApprovalGoalsQuery(999);

    // Act
    var result = await sut.Handle(query, CancellationToken.None);

    // Assert
    Assert.Equal(2, result.Count);
    Assert.Contains(result, g => g.TeamId == 10 && g.TeamName == "Team-A" && g.User == "user1@example.com");
    Assert.Contains(result, g => g.TeamId == 20 && g.TeamName == "Team-B" && g.User == "user2@example.com");
    await id.Received(1).GetUserEmails(Arg.Is<IList<int>>(u => u.Count == 2)); // distinct users
  }

  [Fact]
  public async Task Handle_Sets_empty_strings_when_team_or_user_not_found()
  {
    // Arrange
    var teamIds = new List<int> { 30 };
    var pending = new List<PendingApprovalGoalDto>
    {
      new() { TeamId = 30, UserId = 5, GoalId = 300, GoalSetId = 3000, GoalTitle = "G3" }
    };

    var goalMgmt = Substitute.For<IGoalManagementQueryService>();
    var org = Substitute.For<IOrganisationQueryService>();
    var id = Substitute.For<IIdentityQueryService>();

    org.GetTeamLeaderTeamIds(Arg.Any<int>()).Returns(teamIds);
    goalMgmt.GetPendingApprovalGoals(teamIds).Returns(pending);
    org.GetTeamNamesAsync(teamIds).Returns(new Dictionary<int, string>()); // missing team name
    id.GetUserEmails(Arg.Any<IList<int>>()).Returns(new Dictionary<int, string>()); // missing user email

    var sut = CreateHandler(goalMgmt, org, id);
    var query = new GetPendingApprovalGoalsQuery(42);

    // Act
    var result = await sut.Handle(query, CancellationToken.None);

    // Assert
    var dto = Assert.Single(result);
    Assert.Equal(string.Empty, dto.TeamName); // set to empty when missing
    Assert.Equal(string.Empty, dto.User); // set to empty when missing
  }

  private static GetPendingApprovalGoalsQueryHandler CreateHandler(
    IGoalManagementQueryService? goalMgmt = null,
    IOrganisationQueryService? org = null,
    IIdentityQueryService? id = null)
    => new(goalMgmt ?? Substitute.For<IGoalManagementQueryService>(),
           org ?? Substitute.For<IOrganisationQueryService>(),
           id ?? Substitute.For<IIdentityQueryService>());
}
