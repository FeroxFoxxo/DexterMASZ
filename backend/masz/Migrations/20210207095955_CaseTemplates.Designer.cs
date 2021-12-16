﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MASZ.Data;

namespace MASZ.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20210207095955_CaseTemplates")]
    partial class CaseTemplates
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("masz.Models.AutoModerationConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("AutoModerationAction")
                        .HasColumnType("int");

                    b.Property<int>("AutoModerationType")
                        .HasColumnType("int");

                    b.Property<string>("GuildId")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("IgnoreChannels")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("IgnoreRoles")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<int?>("Limit")
                        .HasColumnType("int");

                    b.Property<int?>("PunishmentDurationMinutes")
                        .HasColumnType("int");

                    b.Property<int?>("PunishmentType")
                        .HasColumnType("int");

                    b.Property<bool>("SendDmNotification")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("SendPublicNotification")
                        .HasColumnType("tinyint(1)");

                    b.Property<int?>("TimeLimitMinutes")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("AutoModerationConfigs");
                });

            modelBuilder.Entity("masz.Models.AutoModerationEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int?>("AssociatedCaseId")
                        .HasColumnType("int");

                    b.Property<int>("AutoModerationAction")
                        .HasColumnType("int");

                    b.Property<int>("AutoModerationType")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Discriminator")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("GuildId")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("MessageContent")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("MessageId")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Nickname")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("UserId")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Username")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.ToTable("AutoModerationEvents");
                });

            modelBuilder.Entity("masz.Models.CaseTemplate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("CaseDescription")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("CaseLabels")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<DateTime?>("CasePunishedUntil")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("CasePunishment")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<int>("CasePunishmentType")
                        .HasColumnType("int");

                    b.Property<string>("CaseTitle")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("TemplateName")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("UserId")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<bool>("announceDm")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("handlePunishment")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("sendPublicNotification")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("Id");

                    b.ToTable("CaseTemplates");
                });

            modelBuilder.Entity("masz.Models.GuildConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("AdminRoleId")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("GuildId")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("ModInternalNotificationWebhook")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<bool>("ModNotificationDM")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("ModPublicNotificationWebhook")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("ModRoleId")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("MutedRoleId")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.ToTable("GuildConfigs");
                });

            modelBuilder.Entity("masz.Models.ModCase", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("CaseId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("CreationType")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Discriminator")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("GuildId")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Labels")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<DateTime>("LastEditedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("LastEditedByModId")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("ModId")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Nickname")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<DateTime>("OccuredAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Others")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<DateTime?>("PunishedUntil")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Punishment")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<bool>("PunishmentActive")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("PunishmentType")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("UserId")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Username")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<bool>("Valid")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("Id");

                    b.ToTable("ModCases");
                });

            modelBuilder.Entity("masz.Models.ModCaseComment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Message")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<int>("ModCaseId")
                        .HasColumnType("int");

                    b.Property<string>("UserId")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.HasIndex("ModCaseId");

                    b.ToTable("ModCaseComments");
                });

            modelBuilder.Entity("masz.Models.ModCaseComment", b =>
                {
                    b.HasOne("masz.Models.ModCase", "ModCase")
                        .WithMany("Comments")
                        .HasForeignKey("ModCaseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
