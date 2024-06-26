﻿using AutoMods;
using Bot;
using Bot.Abstractions;
using Greeting;
using GuildAudits;
using Invites;
using JoinLeave;
using Levels;
using Messaging;
using MOTDs;
using Music;
using PrivateVCs;
using Punishments;
using RoleReactions;
using UserMaps;
using UserNotes;
using Utilities;

namespace Launch;

public static class ImportModules
{
    public static List<Module> GetModules() =>
        new()
        {
            new BotModule(),
            new AutoModModule(),
            new GreeterModule(),
            new GuildAuditModule(),
            new InviteModule(),
            new JoinLeaveModule(),
            new MotdModule(),
            new MusicModule(),
            new PunishmentModule(),
            new PrivateVcModule(),
            new RoleReactionsModule(),
            new UserMapModule(),
            new UserNoteModule(),
            new UtilityModule(),
            new MessagingModule(),
            new LevelsModule(),
        };
}
