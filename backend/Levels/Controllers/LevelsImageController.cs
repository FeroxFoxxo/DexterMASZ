﻿using Bot.Abstractions;
using Bot.DTOs;
using Bot.Exceptions;
using Bot.Services;
using Levels.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Levels.Controllers;

[Route("api/v1/levels/{userId}/images")]
public class LevelsImageController(IdentityManager identityManager, LevelsImageRepository levelsImageRepository,
    UserRankcardConfigRepository levelsRankcardRepository) : AuthenticatedController(identityManager, levelsImageRepository, levelsRankcardRepository)
{
    private const int MaximumFileSize = 10485760; //10 MiB
    private readonly LevelsImageRepository _levelsImageRepository = levelsImageRepository;
    private readonly UserRankcardConfigRepository _levelsRankcardRepository = levelsRankcardRepository;

    [HttpDelete("{fileName}")]
    public async Task<IActionResult> DeleteImage([FromRoute] ulong userId, [FromRoute] string fileName)
    {
        var identity = await SetupAuthentication();

        if (identity.GetCurrentUser().Id != userId && !await identity.IsSiteAdmin())
            throw new UnauthorizedException("Insufficient permissions to delete another user's files");

        await _levelsImageRepository.DeleteFile(userId, fileName);
        return Ok();
    }

    [Authorize]
    [HttpPost]
    [RequestSizeLimit(MaximumFileSize)]
    public async Task<IActionResult> PostImage([FromRoute] ulong userId, [FromForm] UploadedFileDto uploadedFile)
    {
        var identity = await SetupAuthentication();

        if (identity.GetCurrentUser().Id != userId && !await identity.IsSiteAdmin())
            throw new UnauthorizedException(
                "Insufficient permissions to delete another user's rankcard configuration.");

        string path;
        try
        {
            path = await _levelsImageRepository.UploadFile(userId, uploadedFile.File);
        }
        catch (BadImageFormatException e)
        {
            return StatusCode(415, e.Message);
        }

        var currentBg = _levelsRankcardRepository.GetOrDefaultRankcard(userId).Background;
        var otherImages = await _levelsImageRepository.GetUserFiles(userId);

        foreach (var fileName in otherImages)
        {
            if (path.EndsWith(fileName)) continue;
            if (currentBg.EndsWith(fileName)) continue;

            await _levelsImageRepository.DeleteFile(userId, fileName);
        }

        return Ok(new FileNameContainer { FileName = Path.GetFileName(path) });
    }

    public class FileNameContainer
    {
        public string FileName { get; set; } = "";
    }
}
