﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RoleReactions.Data;

#nullable disable

namespace RoleReactions.Migrations
{
    [DbContext(typeof(RoleReactionsDatabase))]
    [Migration("20231024035619_MaximumRoles")]
    partial class MaximumRoles
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("RoleReactions")
                .HasAnnotation("ProductVersion", "7.0.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("RoleReactions.Models.RoleMenu", b =>
                {
                    b.Property<ulong>("GuildId")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("ChannelId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<int>("MaximumRoles")
                        .HasColumnType("int");

                    b.Property<ulong>("MessageId")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.Property<string>("RoleToEmote")
                        .HasColumnType("longtext");

                    b.HasKey("GuildId", "ChannelId", "Id");

                    b.ToTable("RoleReactionsMenu", "RoleReactions");
                });

            modelBuilder.Entity("RoleReactions.Models.UserRoles", b =>
                {
                    b.Property<ulong>("GuildId")
                        .HasColumnType("bigint unsigned");

                    b.Property<ulong>("ChannelId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("RoleIds")
                        .HasColumnType("longtext");

                    b.HasKey("GuildId", "ChannelId", "Id", "UserId");

                    b.ToTable("UserRoles", "RoleReactions");
                });
#pragma warning restore 612, 618
        }
    }
}
