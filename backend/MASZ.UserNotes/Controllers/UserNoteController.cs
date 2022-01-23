using MASZ.Bot.Abstractions;
using MASZ.Bot.Enums;
using MASZ.Bot.Services;
using MASZ.UserNotes.Data;
using MASZ.UserNotes.DTOs;
using MASZ.UserNotes.Views;
using Microsoft.AspNetCore.Mvc;

namespace MASZ.UserNotes.Controllers;

[Route("api/v1/guilds/{guildId}/usernote")]
public class UserNoteController : AuthenticatedController
{
	private readonly UserNoteRepository _userNoteRepo;

	public UserNoteController(IdentityManager identityManager, UserNoteRepository userNoteRepo) :
		base(identityManager, userNoteRepo)
	{
		_userNoteRepo = userNoteRepo;
	}

	[HttpGet]
	public async Task<IActionResult> GetUserNote([FromRoute] ulong guildId)
	{
		var identity = await SetupAuthentication();

		await identity.RequirePermission(DiscordPermission.Moderator, guildId);

		var userNotes = await _userNoteRepo.GetUserNotesByGuild(guildId);

		return Ok(userNotes.Select(x => new UserNoteView(x)));
	}

	[HttpGet("{userId}")]
	public async Task<IActionResult> GetUserNote([FromRoute] ulong guildId, [FromRoute] ulong userId)
	{
		var identity = await SetupAuthentication();

		await identity.RequirePermission(DiscordPermission.Moderator, guildId);

		return Ok(new UserNoteView(await _userNoteRepo.GetUserNote(guildId, userId)));
	}

	[HttpPut]
	public async Task<IActionResult> CreateUserNote([FromRoute] ulong guildId, [FromBody] UserNoteForUpdateDto userNote)
	{
		var identity = await SetupAuthentication();

		await identity.RequirePermission(DiscordPermission.Moderator, guildId);

		var createdUserNote =
			await _userNoteRepo.CreateOrUpdateUserNote(guildId, userNote.UserId, userNote.Description);

		return StatusCode(201, new UserNoteView(createdUserNote));
	}

	[HttpDelete("{userId}")]
	public async Task<IActionResult> DeleteUserNote([FromRoute] ulong guildId, [FromRoute] ulong userId)
	{
		var identity = await SetupAuthentication();

		await identity.RequirePermission(DiscordPermission.Moderator, guildId);

		await _userNoteRepo.DeleteUserNote(guildId, userId);

		return Ok();
	}
}