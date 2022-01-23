﻿using Discord;
using MASZ.Bot.Abstractions;
using MASZ.Bot.Enums;

namespace MASZ.AutoMods.Translators;

public class AutoModNotificationTranslator : Translator
{
	public string NotificationAutoModerationInternal(IUser user)
	{
		return PreferredLanguage switch
		{
			Language.De => $"{user.Mention} hat die AutoModeration ausgelöst.",
			Language.At => $"{user.Mention} hot de Automodaration ausglest.",
			Language.Fr => $"{user.Mention} a déclenché l'automodération.",
			Language.Es => $"{user.Mention} activó la automoderación.",
			Language.Ru => $"{user.Mention} запустил автомодерацию.",
			Language.It => $"{user.Mention} ha attivato la moderazione automatica.",
			_ => $"{user.Mention} triggered AutoModeration."
		};
	}

	public string NotificationAutoModerationCase(IUser user)
	{
		return PreferredLanguage switch
		{
			Language.De => $"{user.Username}#{user.Discriminator} hat die AutoModeration ausgelöst.",
			Language.At => $"{user.Username}#{user.Discriminator} hot de Automodaration ausglest.",
			Language.Fr => $"{user.Username}#{user.Discriminator} a déclenché la modération automatique.",
			Language.Es => $"{user.Username}#{user.Discriminator} desencadenó la automoderación.",
			Language.Ru => $"{user.Username}#{user.Discriminator} запускает автомодерацию.",
			Language.It => $"{user.Username}#{user.Discriminator} ha attivato la moderazione automatica.",
			_ => $"{user.Username}#{user.Discriminator} triggered AutoModeration."
		};
	}

	public string NotificationAutoModerationDm(IUser user, IMentionable channel, string reason, string action)
	{
		return PreferredLanguage switch
		{
			Language.De =>
				$"Hallo {user.Mention},\n\nDu hast die AutoModeration in {channel.Mention} ausgelöst.\nGrund: {reason}\nAktion: {action}",
			Language.At =>
				$"Servus {user.Mention},\n\nDu host de Automodaration in {channel.Mention} ausglest. Grund: {reason}\nAktion: {action}",
			Language.Fr =>
				$"Salut {user.Mention},\n\nVous avez déclenché l'automodération dans {channel.Mention}.\nRaison : {reason}\nAction : {action}",
			Language.Es =>
				$"Hola, {user.Mention}:\n\nActivó la automoderación en {channel.Mention}.\nMotivo: {reason}\nAcción: {action}",
			Language.Ru =>
				$"Привет, {user.Mention}!\n\nВы активировали автомодерацию в {channel.Mention}.\nПричина: {reason}\nДействие: {action}",
			Language.It =>
				$"Ciao {user.Mention},\n\nHai attivato la moderazione automatica in {channel.Mention}.\nMotivo: {reason}\nAzione: {action}",
			_ =>
				$"Hi {user.Mention},\n\nYou triggered AutoModeration in {channel.Mention}.\nReason: {reason}\nAction: {action}"
		};
	}

	public string NotificationAutoModerationChannel(IUser user, string reason)
	{
		return PreferredLanguage switch
		{
			Language.De =>
				$"{user.Mention} du hast die AutoModeration ausgelöst. Grund: {reason}. Deine Nachricht wurde gelöscht.",
			Language.At =>
				$"{user.Mention} du host de Automodaration ausglest. Grund: {reason}. Dei Nochricht wuad glescht.",
			Language.Fr =>
				$"{user.Mention} vous avez déclenché l'automodération. Raison : {reason}. Votre message a été supprimé.",
			Language.Es =>
				$"{user.Mention} has activado la automoderación. Razón: {reason}. Su mensaje ha sido eliminado.",
			Language.Ru =>
				$"{user.Mention} вы запустили автомодерацию. Причина: {reason}. Ваше сообщение было удалено.",
			Language.It =>
				$"{user.Mention} hai attivato la moderazione automatica. reason: {reason}. Il tuo messaggio è stato cancellato.",
			_ => $"{user.Mention} you triggered AutoModeration. Reason: {reason}. Your message has been deleted."
		};
	}

	public string NotificationAutoModerationConfigInternalCreate(string eventType, IUser actor)
	{
		return PreferredLanguage switch
		{
			Language.De => $"Automodkonfiguration für {eventType} von {actor.Mention} erstellt.",
			Language.At => $"Automodkonfiguration fia {eventType} vo {actor.Mention} erstöt.",
			Language.Fr => $"Automodconfig créé pour {eventType} par {actor.Mention}.",
			Language.Es => $"Automodconfig creado para {eventType} por {actor.Mention}.",
			Language.Ru => $"Automodconfig, созданный для {eventType} пользователем {actor.Mention}.",
			Language.It => $"Automodconfig creato per {eventType} da {actor.Mention}.",
			_ => $"Automodconfig created for {eventType} by {actor.Mention}."
		};
	}

	public string NotificationAutoModerationConfigInternalUpdate(string eventType, IUser actor)
	{
		return PreferredLanguage switch
		{
			Language.De => $"Automodkonfiguration für {eventType} von {actor.Mention} bearbeitet.",
			Language.At => $"Automodkonfiguration fia {eventType} is vo {actor.Mention} beorbeit woan.",
			Language.Fr => $"Automodconfig mis à jour pour {eventType} par {actor.Mention}.",
			Language.Es => $"Automodconfig actualizado para {eventType} por {actor.Mention}.",
			Language.Ru => $"Automodconfig обновлен для {eventType} пользователем {actor.Mention}.",
			Language.It => $"Automodconfig aggiornato per {eventType} da {actor.Mention}.",
			_ => $"Automodconfig updated for {eventType} by {actor.Mention}."
		};
	}

	public string NotificationAutoModerationConfigInternalDelete(string eventType, IUser actor)
	{
		return PreferredLanguage switch
		{
			Language.De => $"Automodkonfiguration für {eventType} von {actor.Mention} gelöscht.",
			Language.At => $"Automodkonfiguration fia {eventType} vo {actor.Mention} glescht.",
			Language.Fr => $"Automodconfig supprimé pour {eventType} par {actor.Mention}.",
			Language.Es => $"Automodconfig eliminado para {eventType} por {actor.Mention}.",
			Language.Ru => $"Automodconfig удален для {eventType} пользователем {actor.Mention}.",
			Language.It => $"Automodconfig eliminato per {eventType} da {actor.Mention}.",
			_ => $"Automodconfig deleted for {eventType} by {actor.Mention}."
		};
	}

	public string NotificationAutoModerationConfigLimit()
	{
		return PreferredLanguage switch
		{
			Language.De => "Limit",
			Language.At => "Limit",
			Language.Fr => "Limite",
			Language.Es => "Límite",
			Language.Ru => "Предел",
			Language.It => "Limite",
			_ => "Limit"
		};
	}

	public string NotificationAutoModerationConfigTimeLimit()
	{
		return PreferredLanguage switch
		{
			Language.De => "Zeitlimit",
			Language.At => "Zeitlimit",
			Language.Fr => "Limite de temps",
			Language.Es => "Límite de tiempo",
			Language.Ru => "Лимит времени",
			Language.It => "Limite di tempo",
			_ => "Time limit"
		};
	}

	public string NotificationAutoModerationConfigIgnoredRoles()
	{
		return PreferredLanguage switch
		{
			Language.De => "Ignorierte Rollen",
			Language.At => "Ignoriate Rolln",
			Language.Fr => "Rôles ignorés",
			Language.Es => "Roles ignorados",
			Language.Ru => "Игнорируемые роли",
			Language.It => "Ruoli ignorati",
			_ => "Ignored roles"
		};
	}

	public string NotificationAutoModerationConfigIgnoredChannels()
	{
		return PreferredLanguage switch
		{
			Language.De => "Ignorierte Kanäle",
			Language.At => "Ignoriate Kanäle",
			Language.Fr => "Canaux ignorés",
			Language.Es => "Canales ignorados",
			Language.Ru => "Игнорируемые каналы",
			Language.It => "Canali ignorati",
			_ => "Ignored channels"
		};
	}

	public string NotificationAutoModerationConfigDuration()
	{
		return PreferredLanguage switch
		{
			Language.De => "Dauer",
			Language.At => "Daua",
			Language.Fr => "Durée",
			Language.Es => "Duración",
			Language.Ru => "Продолжительность",
			Language.It => "Durata",
			_ => "Duration"
		};
	}

	public string NotificationAutoModerationConfigDeleteMessage()
	{
		return PreferredLanguage switch
		{
			Language.De => "Nachricht löschen",
			Language.At => "Nochricht leschn",
			Language.Fr => "Supprimer le message",
			Language.Es => "Borrar mensaje",
			Language.Ru => "Удаленное сообщение",
			Language.It => "Cancella il messaggio",
			_ => "Delete message"
		};
	}

	public string NotificationAutoModerationConfigSendPublic()
	{
		return PreferredLanguage switch
		{
			Language.De => "Sende öffentliche Nachricht",
			Language.At => "Schick a öffentliche Nochricht",
			Language.Fr => "Envoyer une notification publique",
			Language.Es => "Enviar notificación pública",
			Language.Ru => "Отправить публичное уведомление",
			Language.It => "Invia notifica pubblica",
			_ => "Send public notification"
		};
	}

	public string NotificationAutoModerationConfigSendDm()
	{
		return PreferredLanguage switch
		{
			Language.De => "Sende DM Nachricht",
			Language.At => "Schick a Direktnachricht",
			Language.Fr => "Envoyer une notification DM",
			Language.Es => "Enviar notificación DM",
			Language.Ru => "Отправить уведомление в прямом эфире",
			Language.It => "Invia notifica DM",
			_ => "Send DM notification"
		};
	}
}