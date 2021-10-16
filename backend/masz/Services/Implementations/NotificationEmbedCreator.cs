using System;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using masz.Models;
using Microsoft.Extensions.Logging;
using masz.Enums;
using masz.Extensions;

namespace masz.Services
{
    public class NotificationEmbedCreator : INotificationEmbedCreator
    {
        private readonly ILogger<Scheduler> _logger;
        private readonly IInternalConfiguration _config;
        private readonly ITranslator _translator;
        private readonly string SCALES_EMOTE = "\u2696";
        private readonly string SCROLL_EMOTE = "\uD83C\uDFF7";
        private readonly string ALARM_CLOCK = "\u23F0";
        private readonly string STAR = "\u2B50";
        private readonly string GLOBE = "\uD83C\uDF0D";
        private readonly string CLOCK = "\uD83D\uDD50";
        private readonly string HAND_SHAKE = "\uD83E\uDD1D";

        public NotificationEmbedCreator(ILogger<Scheduler> logger, IInternalConfiguration config, ITranslator translator)
        {
            _logger = logger;
            _translator = translator;
            _config = config;
        }

        private DiscordEmbedBuilder CreateBasicEmbed(RestAction action, DiscordUser author = null)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder();

            embed.Timestamp = DateTime.Now;

            switch(action) {
                case RestAction.Edited:
                    embed.Color = DiscordColor.Orange;
                    break;
                case RestAction.Deleted:
                    embed.Color = DiscordColor.Red;
                    break;
                case RestAction.Created:
                    embed.Color = DiscordColor.Green;
                    break;
            }

            if (author != null) {
                embed.WithAuthor(
                    author.Username,
                    author.AvatarUrl,
                    author.AvatarUrl
                );
            }

            // Url
            if (! string.IsNullOrEmpty(_config.GetBaseUrl()))
            {
                embed.Url = _config.GetBaseUrl();
            }

            return embed;
        }

        public async Task<DiscordEmbedBuilder> CreateModcaseEmbed(ModCase modCase, RestAction action, DiscordUser actor, DiscordUser suspect = null, bool isInternal = true)
        {
            await _translator.SetContext(modCase.GuildId);
            DiscordEmbedBuilder embed;
            if (isInternal) {
                embed = CreateBasicEmbed(action);
            } else {
                embed = CreateBasicEmbed(action, actor);
            }

            // Thumbnail
            if (suspect != null)
            {
                embed.WithThumbnail(suspect.AvatarUrl);
            }

            // Description
            embed.AddField($"**{_translator.T().Description()}**", modCase.Description.Truncate(1000));

            // Title
            embed.Title = $"#{modCase.CaseId} {modCase.Title}";

            // Footer
            embed.WithFooter($"UserId: {modCase.UserId} | CaseId: {modCase.CaseId}");

            // Description
            switch(action){
                case RestAction.Edited:
                    if (isInternal) {
                        embed.Description = _translator.T().NotificationModcaseUpdateInternal(modCase, actor);
                    } else {
                        embed.Description = _translator.T().NotificationModcaseUpdatePublic(modCase);
                    }
                    break;
                case RestAction.Deleted:
                    if (isInternal) {
                        embed.Description = _translator.T().NotificationModcaseDeleteInternal(modCase, actor);
                    } else {
                        embed.Description = _translator.T().NotificationModcaseDeletePublic(modCase);
                    }
                    break;
                case RestAction.Created:
                    if (isInternal) {
                        embed.Description = _translator.T().NotificationModcaseCreateInternal(modCase, actor);
                    } else {
                        embed.Description = _translator.T().NotificationModcaseCreatePublic(modCase);
                    }
                    break;
            }

            // Punishment
            embed.AddField($"{SCALES_EMOTE} - {_translator.T().Punishment()}", modCase.GetPunishment(_translator), true);
            if (modCase.PunishedUntil != null)
            {
                embed.AddField($"{ALARM_CLOCK} - {_translator.T().PunishmentUntil()}", modCase.PunishedUntil.Value.ToDiscordTS(), true);
            }

            // Labels
            if (modCase.Labels.Length != 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach(string label in modCase.Labels)
                {
                    sb.Append($"`{label}` ");
                    if (sb.ToString().Length > 1000) {
                        break;
                    }
                }
                embed.AddField($"{SCROLL_EMOTE} - {_translator.T().Labels()}", sb.ToString(), modCase.PunishedUntil == null);
            }

            return embed;
        }

        public async Task<DiscordEmbedBuilder> CreateFileEmbed(string filename, ModCase modCase, RestAction action, DiscordUser actor)
        {
            await _translator.SetContext(modCase.GuildId);
            DiscordEmbedBuilder embed = CreateBasicEmbed(action, actor);

            // Thumbnail
            embed.WithThumbnail(actor.AvatarUrl);

            // Footer
            embed.WithFooter($"UserId: {actor.Id} | CaseId: {modCase.CaseId}");

            // Filename
            embed.AddField($"**{_translator.T().Filename()}**", filename.Truncate(1000));

            switch(action){
                case RestAction.Edited:
                    embed.Description = _translator.T().NotificationModcaseFileUpdate(actor);
                    embed.Title = $"**{_translator.T().NotificationFilesUpdate().ToUpper()}** - #{modCase.CaseId} {modCase.Title}";
                    break;
                case RestAction.Deleted:
                    embed.Description = _translator.T().NotificationModcaseFileDelete(actor);
                    embed.Title = $"**{_translator.T().NotificationFilesDelete().ToUpper()}** - #{modCase.CaseId} {modCase.Title}";
                    break;
                case RestAction.Created:
                    embed.Description = _translator.T().NotificationModcaseFileCreate(actor);
                    embed.Title = $"**{_translator.T().NotificationFilesCreate().ToUpper()}** - #{modCase.CaseId} {modCase.Title}";
                    break;
            }

            return embed;
        }

        public async Task<DiscordEmbedBuilder> CreateCommentEmbed(ModCaseComment comment, RestAction action, DiscordUser actor)
        {
            await _translator.SetContext(comment.ModCase.GuildId);
            DiscordEmbedBuilder embed = CreateBasicEmbed(action, actor);

            if (actor != null)
            {
                embed.WithThumbnail(actor.AvatarUrl);
            }

            switch(action){
                case RestAction.Edited:
                    embed.Description = _translator.T().NotificationModcaseCommentsUpdate(actor);
                    embed.Title = $"**{_translator.T().NotificationModcaseCommentsShortUpdate().ToUpper()}** - #{comment.ModCase.CaseId} {comment.ModCase.Title}";
                    break;
                case RestAction.Deleted:
                    embed.Description = _translator.T().NotificationModcaseCommentsDelete(actor);
                    embed.Title = $"**{_translator.T().NotificationModcaseCommentsShortDelete().ToUpper()}** - #{comment.ModCase.CaseId} {comment.ModCase.Title}";
                    break;
                case RestAction.Created:
                    embed.Description = _translator.T().NotificationModcaseCommentsCreate(actor);
                    embed.Title = $"**{_translator.T().NotificationModcaseCommentsShortCreate().ToUpper()}** - #{comment.ModCase.CaseId} {comment.ModCase.Title}";
                    break;
            }

            // Message
            embed.AddField($"**{_translator.T().Message()}**", comment.Message.Truncate(1000));

            // Footer
            embed.WithFooter($"UserId: {actor.Id} | CaseId: {comment.ModCase.CaseId}");

            return embed;
        }

        public async Task<DiscordEmbedBuilder> CreateUserNoteEmbed(UserNote userNote, RestAction action, DiscordUser actor, DiscordUser target)
        {
            await _translator.SetContext(userNote.GuildId);
            DiscordEmbedBuilder embed = CreateBasicEmbed(action, actor);

            if (actor != null)
            {
                embed.WithThumbnail(target.AvatarUrl);
            }

            embed.AddField($"**{_translator.T().Description()}**", userNote.Description.Truncate(1000));

            embed.Title = $"{_translator.T().UserNote()} #{userNote.Id}";

            embed.WithFooter($"UserId: {userNote.UserId} | UserNoteId: {userNote.Id}");

            return embed;
        }

        public async Task<DiscordEmbedBuilder> CreateUserMapEmbed(UserMapping userMapping, RestAction action, DiscordUser actor)
        {
            await _translator.SetContext(userMapping.GuildId);
            DiscordEmbedBuilder embed = CreateBasicEmbed(action, actor);

            embed.AddField($"**{_translator.T().Description()}**", userMapping.Reason.Truncate(1000));

            embed.Title = $"{_translator.T().UserMap()} #{userMapping.Id}";
            embed.Description = _translator.T().UserMapBetween(userMapping);

            embed.WithFooter($"UserA: {userMapping.UserA} | UserB: {userMapping.UserB} | UserMapId: {userMapping.Id}");

            return embed;
        }

        public DiscordEmbedBuilder CreateTipsEmbedForNewGuilds(GuildConfig guildConfig)
        {
            _translator.SetContext(guildConfig);
            DiscordEmbedBuilder embed = CreateBasicEmbed(RestAction.Created, null);

            embed.Title = _translator.T().NotificationRegisterWelcomeToMASZ();
            embed.Description = _translator.T().NotificationRegisterDescriptionThanks();

            embed.AddField(
                $"{STAR} {_translator.T().Features()}",
                _translator.T().NotificationRegisterUseFeaturesCommand(),
                false
            );

            embed.AddField(
                $"{GLOBE} {_translator.T().LanguageWord()}",
                _translator.T().NotificationRegisterDefaultLanguageUsed(guildConfig.PreferredLanguage.ToString()),
                false
            );

            embed.AddField(
                $"{CLOCK} {_translator.T().Timestamps()}",
                _translator.T().NotificationRegisterConfusingTimestamps(),
                false
            );

            embed.AddField(
                $"{HAND_SHAKE} {_translator.T().Support()}",
                _translator.T().NotificationRegisterSupport(),
                false
            );

            embed.WithFooter($"GuildId: {guildConfig.GuildId}");

            return embed;
        }

        public DiscordEmbedBuilder CreateInternalAutomodEmbed(AutoModerationEvent autoModerationEvent, GuildConfig guildConfig, DiscordUser user, DiscordChannel channel, PunishmentType? punishmentType = null)
        {
            _translator.SetContext(guildConfig);
            DiscordEmbedBuilder embed = CreateBasicEmbed(RestAction.Created, null);

            embed.Title = _translator.T().Automoderation();
            embed.Description = _translator.T().NotificationAutomoderationInternal(user);

            embed.AddField(
                _translator.T().Channel(),
                channel.Mention,
                true
            );

            embed.AddField(
                _translator.T().Message(),
                autoModerationEvent.MessageId.ToString(),
                true
            );

            embed.AddField(
                _translator.T().Type(),
                _translator.T().Enum(autoModerationEvent.AutoModerationType),
                false
            );

            embed.AddField(
                _translator.T().MessageContent(),
                autoModerationEvent.MessageContent,
                false
            );

            embed.AddField(
                _translator.T().Action(),
                _translator.T().Enum(autoModerationEvent.AutoModerationAction),
                false
            );

            if ((autoModerationEvent.AutoModerationAction == AutoModerationAction.CaseCreated ||
                 autoModerationEvent.AutoModerationAction == AutoModerationAction.ContentDeletedAndCaseCreated) &&
                punishmentType != null)
            {
                embed.AddField(
                    _translator.T().Punishment(),
                    _translator.T().Enum(punishmentType.Value),
                    false
                );
            }

            embed.WithFooter($"GuildId: {guildConfig.GuildId}");

            return embed;
        }
    }
}