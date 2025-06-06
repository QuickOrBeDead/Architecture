﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using GoalManager.UseCases.Identity.GetUserLookup;
using GoalManager.UseCases.Organisation.AddTeamMember;
using GoalManager.UseCases.Organisation.GetTeamMemberTypeLookup;
using GoalManager.UseCases.Organisation.GetTeamName;
using GoalManager.Web.Common;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GoalManager.Web.Pages.Organisation;

[Authorize]
public class AddMemberModel(IMediator mediator) : PageModelBase
{
  public string? TeamName { get; set; }

  [DisplayName("User")]
  [Required]
  [BindProperty]
  public int? UserId { get; set; }

  [DisplayName("Member Type")]
  [Required]
  [BindProperty]
  public int? MemberTypeId { get; set; }

  public List<SelectListItem> Users { get; set; } = new(0);

  public List<SelectListItem> MemberTypes { get; set; } = new(0);

  public async Task<IActionResult> OnGetAsync(int teamId)
  {
    var teamNameResult = await mediator.Send(new GetTeamNameQuery(teamId)).ConfigureAwait(false);

    AddResultMessages(teamNameResult);

    if (teamNameResult.IsSuccess)
    {
      TeamName = teamNameResult.Value;
    }

    var usersResult = await mediator.Send(new GetUserLookupQuery()).ConfigureAwait(false);

    AddResultMessages(usersResult);

    if (usersResult.IsSuccess)
    {
      var users = usersResult.Value.Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToList();
      //Users.Add(new SelectListItem("Choose..", string.Empty));
      Users.AddRange(users);
    }

    var memberTypesResult = await mediator.Send(new GetTeamMemberTypeLookupQuery()).ConfigureAwait(false);

    AddResultMessages(memberTypesResult);

    if (memberTypesResult.IsSuccess)
    {
      var memberTypes = memberTypesResult.Value.Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToList();
      //MemberTypes.Add(new SelectListItem("Choose..", string.Empty));
      MemberTypes.AddRange(memberTypes);
    }

    return Page();
  }

  public async Task<IActionResult> OnPostAsync(int organisationId, int teamId)
  {
    if (!ModelState.IsValid)
    {
      return await OnGetAsync(teamId).ConfigureAwait(false);
    }

    var result = await mediator.Send(new AddTeamMemberCommand(organisationId, teamId, UserId.GetValueOrDefault(), MemberTypeId.GetValueOrDefault())).ConfigureAwait(false);

    AddResultMessages(result);

    if (!result.IsSuccess)
    {
      return await OnGetAsync(teamId).ConfigureAwait(false);
    }

    return RedirectToPageWithSuccessMessage("UpdateTeam", new {organisationId, teamId}, "Team Member is Added");
  }
}
