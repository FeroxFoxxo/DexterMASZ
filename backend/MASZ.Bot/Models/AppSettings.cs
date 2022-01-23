﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MASZ.Bot.Enums;

namespace MASZ.Bot.Models;

public class AppSettings
{
	[Key] public ulong ClientId { get; set; }
	public string DiscordBotToken { get; set; }
	public string ClientSecret { get; set; }
	public string AbsolutePathToFileUpload { get; set; }
	public string ServiceHostName { get; set; }
	public string ServiceDomain { get; set; }
	public string ServiceBaseUrl { get; set; }
	public ulong[] SiteAdmins { get; set; }
	public string AuditLogWebhookUrl { get; set; } = string.Empty;
	public bool PublicFileMode { get; set; } = false;
	public bool DemoModeEnabled { get; set; } = false;
	public bool CorsEnabled { get; set; } = false;
	[JsonIgnore] public string Lang { get; set; } = "en";
	public string EmbedTitle { get; set; } = "MASZ - a discord moderation bot";
    public string EmbedContent { get; set; } = "MASZ is a moderation bot for Discord Moderators. Keep track of all moderation events on your server, search reliably for infractions or setup automoderation to be one step ahead of trolls and rule breakers.";

	[NotMapped]
	public Language DefaultLanguage
	{
		get
		{
			return Lang switch
			{
				"de" => Language.De,
				"it" => Language.It,
				"fr" => Language.Fr,
				"es" => Language.Es,
				"at" => Language.At,
				"ru" => Language.Ru,
				_ => Language.En
			};
		}
		set
		{
			Lang = value switch
			{
				Language.De => "de",
				Language.It => "it",
				Language.Fr => "fr",
				Language.Es => "es",
				Language.At => "at",
				Language.Ru => "ru",
				_ => "en"
			};
		}
	}

	public string GetEmbedData(string url)
	{
		return
			"<html>" +
				"<head>" +
					"<meta property=\"og:site_name\" content=\"MASZ by zaanposni\" />" +
					"<meta property=\"og:title\" content=\"" + EmbedTitle + "\" />" +
					"<meta property=\"og:url\" content=\"" + url + "\" />" +
					(string.IsNullOrWhiteSpace(EmbedContent) ? "" : "<meta property=\"og:description\" content=\"" + EmbedContent + "\" />") +
				"</head>" +
			"</html>";
	}
}