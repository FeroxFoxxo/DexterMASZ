﻿using Discord;
using MASZ.Bot.Abstractions;
using MASZ.Bot.Enums;
using MASZ.Bot.Extensions;

namespace MASZ.Invites.Translators;

public class InviteTranslator : Translator
{
	public string InviteNotFromThisGuild()
	{
		return PreferredLanguage switch
		{
			Language.De => "Die Einladung ist nicht von dieser Gilde.",
			Language.At => "Die Eiladung is ned vo dera Güde.",
			Language.Fr => "L'invitation n'est pas de cette guilde.",
			Language.Es => "La invitación no es de este gremio.",
			Language.Ru => "Приглашение не из этой гильдии.",
			Language.It => "L'invito non è di questa gilda.",
			_ => "The invite is not from this guild."
		};
	}

	public string CannotFindInvite()
	{
		return PreferredLanguage switch
		{
			Language.De => "Konnte die Einladung nicht in der Datenbank oder in dieser Gilde finden.",
			Language.At => "Konnt de Eiladung ned in da Datnbank vo da Güde findn.",
			Language.Fr => "Impossible de trouver l'invitation dans la base de données ou dans cette guilde.",
			Language.Es => "No se pudo encontrar la invitación en la base de datos o en este gremio.",
			Language.Ru => "Не удалось найти инвайт в базе данных или в этой гильдии.",
			Language.It => "Impossibile trovare l'invito nel database o in questa gilda.",
			_ => "Could not find invite in database or in this guild."
		};
	}

	public string FailedToFetchInvite()
	{
		return PreferredLanguage switch
		{
			Language.De => "Konnte die Einladung nicht abrufen.",
			Language.At => "Konnt die Eiladung ned orufn.",
			Language.Fr => "Échec de la récupération de l'invitation.",
			Language.Es => "No se pudo recuperar la invitación.",
			Language.Ru => "Не удалось получить приглашение.",
			Language.It => "Impossibile recuperare l'invito.",
			_ => "Failed to fetch invite."
		};
	}

	public string CreatedAt(string inviteCode, DateTime createdAt)
	{
		return PreferredLanguage switch
		{
			Language.De => $"`{inviteCode}` erstellt am {createdAt.ToDiscordTs()}.",
			Language.At => $"`{inviteCode}` erstöt vo {createdAt.ToDiscordTs()}.",
			Language.Fr => $"`{inviteCode}` créé à {createdAt.ToDiscordTs()}.",
			Language.Es => $"`{inviteCode}` creado en {createdAt.ToDiscordTs()}.",
			Language.Ru => $"`{inviteCode}` создан в {createdAt.ToDiscordTs()}.",
			Language.It => $"`{inviteCode}` creato su {createdAt.ToDiscordTs()}.",
			_ => $"`{inviteCode}` created at {createdAt.ToDiscordTs()}."
		};
	}

	public string CreatedBy(string inviteCode, IUser createdBy)
	{
		return PreferredLanguage switch
		{
			Language.De => $"`{inviteCode}` erstellt von {createdBy.Mention}.",
			Language.At => $"`{inviteCode}` erstöt vo {createdBy.Mention}.",
			Language.Fr => $"`{inviteCode}` créé par {createdBy.Mention}.",
			Language.Es => $"`{inviteCode}` creado por {createdBy.Mention}.",
			Language.Ru => $"`{inviteCode}` создан {createdBy.Mention}.",
			Language.It => $"`{inviteCode}` creato da {createdBy.Mention}.",
			_ => $"`{inviteCode}` created by {createdBy.Mention}."
		};
	}

	public string CreatedByAt(string inviteCode, IUser createdBy, DateTime createdAt)
	{
		return PreferredLanguage switch
		{
			Language.De => $"`{inviteCode}` erstellt von {createdBy.Mention} am {createdAt.ToDiscordTs()}.",
			Language.At => $"`{inviteCode}` erstöt vo {createdBy.Mention} om {createdAt.ToDiscordTs()}.",
			Language.Fr => $"`{inviteCode}` créé par {createdBy.Mention} à {createdAt.ToDiscordTs()}.",
			Language.Es => $"`{inviteCode}` creado por {createdBy.Mention} en {createdAt.ToDiscordTs()}.",
			Language.Ru => $"`{inviteCode}` создан {createdBy.Mention} в {createdAt.ToDiscordTs()}.",
			Language.It => $"`{inviteCode}` creato da {createdBy.Mention} su {createdAt.ToDiscordTs()}.",
			_ => $"`{inviteCode}` created by {createdBy.Mention} at {createdAt.ToDiscordTs()}."
		};
	}

	public string NotTrackedYet()
	{
		return PreferredLanguage switch
		{
			Language.De => "Diese Einladung wurde noch nicht von MASZ gespeichert.",
			Language.At => "Die Eiladung wuad no ned vo MASZ gspeichat.",
			Language.Fr => "Cette invitation n'a pas encore été suivie par MASZ.",
			Language.Es => "MASZ aún no ha realizado el seguimiento de esta invitación.",
			Language.Ru => "Это приглашение еще не отслежено MASZ.",
			Language.It => "Questo invito non è stato ancora monitorato da MASZ.",
			_ => "This invite has not been tracked by MASZ yet."
		};
	}

	public string UsedInvite(string inviteCode)
	{
		return PreferredLanguage switch
		{
			Language.De => $"Benutzte Einladung `{inviteCode}`.",
			Language.At => $"Benutze Eilodung `{inviteCode}`.",
			Language.Fr => $"Invitation utilisée `{inviteCode}`.",
			Language.Es => $"Invitación usada `{inviteCode}`.",
			Language.Ru => $"Использовал инвайт `{inviteCode}`.",
			Language.It => $"Invito usato `{inviteCode}`.",
			_ => $"Used invite `{inviteCode}`."
		};
	}

	public string ByUser(ulong user)
	{
		return PreferredLanguage switch
		{
			Language.De => $"Von <@{user}>.",
			Language.At => $"Vo <@{user}>",
			Language.Fr => $"Par <@{user}>.",
			Language.Es => $"Por <@{user}>.",
			Language.Ru => $"Автор <@{user}>.",
			Language.It => $"Da <@{user}>.",
			_ => $"By <@{user}>."
		};
	}

	public string UsedBy(int count)
	{
		return PreferredLanguage switch
		{
			Language.De => $"Benutzt von [{count}]",
			Language.At => $"Benutzt vo [{count}]",
			Language.Fr => $"Utilisé par [{count}]",
			Language.Es => $"Usado por [{count}]",
			Language.Ru => $"Используется [{count}]",
			Language.It => $"Utilizzato da [{count}]",
			_ => $"Used by [{count}]"
		};
	}
}