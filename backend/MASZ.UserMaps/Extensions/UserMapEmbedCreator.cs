﻿using Discord;
using Humanizer;
using MASZ.Bot.Enums;
using MASZ.Bot.Extensions;
using MASZ.Bot.Services;
using MASZ.Bot.Translators;
using MASZ.UserMaps.Models;
using MASZ.UserMaps.Translators;
using Microsoft.Extensions.DependencyInjection;

namespace MASZ.UserMaps.Extensions;

public static class UserMapEmbedCreator
{
	public static async Task<EmbedBuilder> CreateUserMapEmbed(this UserMap userMaps, RestAction action, IUser actor,
		IServiceProvider provider)
	{
		var translator = provider.GetRequiredService<Translation>();

		await translator.SetLanguage(userMaps.GuildId);

		var embed = await EmbedCreator.CreateBasicEmbed(action, provider, actor);

		embed.AddField($"**{translator.Get<BotTranslator>().Description()}**", userMaps.Reason.Truncate(1000))
			.WithTitle($"{translator.Get<UserMapTranslator>().UserMap()} #{userMaps.Id}")
			.WithDescription(translator.Get<UserMapTranslator>().UserMapBetween(userMaps));

		embed.WithFooter(
			$"{translator.Get<BotTranslator>().User()} A: {userMaps.UserA} | {translator.Get<BotTranslator>().User()} B: {userMaps.UserB} | {translator.Get<UserMapTranslator>().UserMapId()}: {userMaps.Id}");

		return embed;
	}
}