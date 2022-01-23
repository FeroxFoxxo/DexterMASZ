using MASZ.Bot.Abstractions;
using MASZ.Bot.Enums;
using MASZ.Bot.Exceptions;
using MASZ.Bot.Services;
using MASZ.Punishments.Data;
using MASZ.Punishments.DTOs;
using MASZ.Punishments.Exceptions;
using MASZ.Punishments.Extensions;
using MASZ.Punishments.Views;
using Microsoft.AspNetCore.Mvc;

namespace MASZ.Punishments.Controllers;

[Route("api/v1/guilds/{guildId}/cases/{caseId}/comments")]
public class ModCaseCommentsController : AuthenticatedController
{
	private readonly ModCaseCommentRepository _modCaseCommentRepository;
	private readonly ModCaseRepository _modCaseRepository;

	public ModCaseCommentsController(IdentityManager identityManager, ModCaseRepository modCaseRepository,
		ModCaseCommentRepository modCaseCommentRepository) :
		base(identityManager, modCaseRepository, modCaseCommentRepository)
	{
		_modCaseRepository = modCaseRepository;
		_modCaseCommentRepository = modCaseCommentRepository;
	}

	[HttpPost]
	public async Task<IActionResult> CreateItem([FromRoute] ulong guildId, [FromRoute] int caseId,
		[FromBody] ModCaseCommentForCreateDto comment)
	{
		var identity = await SetupAuthentication();

		var modCase = await _modCaseRepository.GetModCase(guildId, caseId);

		await identity.RequirePermission(ApiActionPermission.View, modCase);

		var currentUser = identity.GetCurrentUser();

		// Suspects can only comment if last comment was not by him.
		if (!await identity.HasPermission(DiscordPermission.Moderator, guildId))
			if (modCase.Comments.Any())
				if (modCase.Comments.Last().UserId == currentUser.Id)
					throw new AlreadyCommentedException();

		var createdComment = await _modCaseCommentRepository.CreateComment(guildId, caseId, comment.Message);

		return StatusCode(201, new CommentsView(createdComment));
	}

	[HttpPut("{commentId}")]
	public async Task<IActionResult> UpdateSpecificItem([FromRoute] ulong guildId, [FromRoute] int caseId,
		[FromRoute] int commentId, [FromBody] ModCaseCommentForPutDto newValue)
	{
		var identity = await SetupAuthentication();

		var modCase = await _modCaseRepository.GetModCase(guildId, caseId);

		await identity.RequirePermission(ApiActionPermission.View, modCase);

		var currentUser = identity.GetCurrentUser();

		var comment = await _modCaseCommentRepository.GetSpecificComment(commentId);

		if (comment.UserId != currentUser.Id && !await identity.IsSiteAdmin())
			throw new UnauthorizedException();

		var createdComment =
			await _modCaseCommentRepository.UpdateComment(guildId, caseId, commentId, newValue.Message);

		return Ok(new CommentsView(createdComment));
	}

	[HttpDelete("{commentId}")]
	public async Task<IActionResult> DeleteSpecificItem([FromRoute] ulong guildId, [FromRoute] int caseId,
		[FromRoute] int commentId)
	{
		var identity = await SetupAuthentication();

		var modCase = await _modCaseRepository.GetModCase(guildId, caseId);

		await identity.RequirePermission(ApiActionPermission.View, modCase);

		var currentUser = identity.GetCurrentUser();

		var comment = await _modCaseCommentRepository.GetSpecificComment(commentId);

		if (comment.UserId != currentUser.Id && !await identity.IsSiteAdmin())
			throw new UnauthorizedException();

		await _modCaseCommentRepository.DeleteComment(guildId, caseId, commentId);

		return Ok();
	}
}