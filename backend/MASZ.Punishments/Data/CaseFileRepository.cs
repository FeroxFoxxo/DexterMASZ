using System.Net.Mime;
using MASZ.Bot.Abstractions;
using MASZ.Bot.Data;
using MASZ.Bot.Exceptions;
using MASZ.Bot.Extensions;
using MASZ.Bot.Models;
using MASZ.Bot.Services;
using MASZ.Punishments.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MASZ.Punishments.Data;

public class CaseFileRepository : Repository
{
	private readonly SettingsRepository _configRepo;
	private readonly PunishmentEventHandler _eventHandler;
	private readonly ILogger<CaseFileRepository> _logger;
	private readonly ModCaseRepository _modCaseRepository;

	public CaseFileRepository(SettingsRepository configRepo,
		PunishmentEventHandler eventHandler, ILogger<CaseFileRepository> logger,
		ModCaseRepository modCaseRepository, DiscordRest discordRest) : base(discordRest)
	{
		_configRepo = configRepo;
		_eventHandler = eventHandler;
		_logger = logger;
		_modCaseRepository = modCaseRepository;

		_configRepo.AsUser(Identity);
		_modCaseRepository.AsUser(Identity);
	}

	public async Task<UploadedFile> GetCaseFile(ulong guildId, int caseId, string fileName)
	{
		var config = await _configRepo.GetAppSettings();

		var filePath = Path.Join(config.AbsolutePathToFileUpload, guildId.ToString(), caseId.ToString(),
			FilesHandler.RemoveSpecialCharacters(fileName));

		var fullFilePath = Path.GetFullPath(filePath);

		// https://stackoverflow.com/a/1321535/9850709
		if (fullFilePath != filePath)
			throw new InvalidPathException();

		byte[] fileData;

		try
		{
			fileData = FilesHandler.ReadFile(filePath);
		}
		catch (Exception e)
		{
			_logger.LogError(e, "Failed to read file");
			throw new ResourceNotFoundException();
		}

		if (fileData == null)
			throw new ResourceNotFoundException();

		var contentType = FilesHandler.GetContentType(filePath);

		var cd = new ContentDisposition
		{
			FileName = fileName,
			Inline = true
		};

		return new UploadedFile
		{
			Name = fileName,
			ContentType = contentType,
			ContentDisposition = cd,
			FileContent = fileData
		};
	}

	public async Task<List<string>> GetCaseFiles(ulong guildId, int caseId)
	{
		var config = await _configRepo.GetAppSettings();

		var uploadDir = Path.Join(config.AbsolutePathToFileUpload, guildId.ToString(), caseId.ToString());

		var fullPath = Path.GetFullPath(uploadDir);

		// https://stackoverflow.com/a/1321535/9850709
		if (fullPath != uploadDir)
			throw new InvalidPathException();

		var files = FilesHandler.GetFilesByDirectory(fullPath);

		if (files == null)
			throw new ResourceNotFoundException();

		return files.Select(f => f.Name).ToList();
	}

	public async Task<string> UploadFile(IFormFile file, ulong guildId, int caseId)
	{
		var config = await _configRepo.GetAppSettings();

		var modCase = await _modCaseRepository.GetModCase(guildId, caseId);

		var uploadDir = Path.Join(config.AbsolutePathToFileUpload, guildId.ToString(), caseId.ToString());

		var fullPath = Path.GetFullPath(uploadDir);

		// https://stackoverflow.com/a/1321535/9850709
		if (fullPath != uploadDir)
			throw new InvalidPathException();

		var fileName = await FilesHandler.SaveFile(file, fullPath);

		_eventHandler.FileUploadedEvent.Invoke(await GetCaseFile(guildId, caseId, fileName), modCase, Identity);

		return fileName;
	}

	public async Task DeleteFile(ulong guildId, int caseId, string fileName)
	{
		var config = await _configRepo.GetAppSettings();

		var modCase = await _modCaseRepository.GetModCase(guildId, caseId);

		var info = await GetCaseFile(guildId, caseId, fileName);

		var filePath = Path.Join(config.AbsolutePathToFileUpload, guildId.ToString(), caseId.ToString(),
			FilesHandler.RemoveSpecialCharacters(fileName));

		var fullFilePath = Path.GetFullPath(filePath);

		// https://stackoverflow.com/a/1321535/9850709
		if (fullFilePath != filePath)
			throw new InvalidPathException();

		if (!FilesHandler.FileExists(fullFilePath))
			throw new ResourceNotFoundException();

		FilesHandler.DeleteFile(fullFilePath);

		_eventHandler.FileDeletedEvent.Invoke(info, modCase, Identity);
	}
}