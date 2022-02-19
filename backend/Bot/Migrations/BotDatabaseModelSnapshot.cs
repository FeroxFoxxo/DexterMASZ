﻿// <auto-generated />
using Bot.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Bot.Migrations
{
    [DbContext(typeof(BotDatabase))]
    partial class BotDatabaseModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("Bot")
                .HasAnnotation("ProductVersion", "6.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Bot.Models.AppSettings", b =>
                {
                    b.Property<ulong>("ClientId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("AbsolutePathToFileUpload")
                        .HasColumnType("longtext");

                    b.Property<string>("AuditLogWebhookUrl")
                        .HasColumnType("longtext");

                    b.Property<string>("ClientSecret")
                        .HasColumnType("longtext");

                    b.Property<bool>("CorsEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("DiscordBotToken")
                        .HasColumnType("longtext");

                    b.Property<string>("EmbedContent")
                        .HasColumnType("longtext");

                    b.Property<string>("EmbedTitle")
                        .HasColumnType("longtext");

                    b.Property<string>("Lang")
                        .HasColumnType("longtext");

                    b.Property<string>("ServiceBaseUrl")
                        .HasColumnType("longtext");

                    b.Property<string>("ServiceDomain")
                        .HasColumnType("longtext");

                    b.Property<string>("SiteAdmins")
                        .HasColumnType("longtext");

                    b.HasKey("ClientId");

                    b.ToTable("AppSettings", "Bot");
                });

            modelBuilder.Entity("Bot.Models.GuildConfig", b =>
                {
                    b.Property<ulong>("GuildId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("AdminRoles")
                        .HasColumnType("longtext");

                    b.Property<string>("AdminWebhook")
                        .HasColumnType("longtext");

                    b.Property<string>("BotChannels")
                        .HasColumnType("longtext");

                    b.Property<bool>("ExecuteWhoIsOnJoin")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("ModNotificationDm")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("ModRoles")
                        .HasColumnType("longtext");

                    b.Property<int>("PreferredLanguage")
                        .HasColumnType("int");

                    b.Property<bool>("PublishModeratorInfo")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("StaffChannels")
                        .HasColumnType("longtext");

                    b.Property<string>("StaffWebhook")
                        .HasColumnType("longtext");

                    b.Property<bool>("StrictModPermissionCheck")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("GuildId");

                    b.ToTable("GuildConfigs", "Bot");
                });
#pragma warning restore 612, 618
        }
    }
}
