﻿using Discord;
using Levels.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Levels.Models;

public class UserRankcardConfig
{
    [Key] public ulong Id { get; set; }

    public uint XpColor { get; set; } = 0xff70cefe;
    public uint OffColor { get; set; } = 0xffffffff;
    public uint LevelBgColor { get; set; } = 0xff202225;
    public uint TitleBgColor { get; set; } = 0xff202225;
    public string Background { get; set; } = "#555555";
    public int TitleOffsetX { get; set; }
    public int TitleOffsetY { get; set; }
    public int LevelOffsetX { get; set; }
    public int LevelOffsetY { get; set; } = 100;
    public int PfpOffsetX { get; set; } = 1000;
    public int PfpOffsetY { get; set; } = 100;
    public float PfpRadiusFactor { get; set; } = 0.9f;

    public RankcardFlags RankcardFlags { get; set; } = RankcardFlags.DisplayPfp | RankcardFlags.PfpBackground |
                                                       RankcardFlags.ClipPfp | RankcardFlags.ShowHybrid;

    [NotMapped]
    public Offset2D TitleOffset
    {
        get => new(TitleOffsetX, TitleOffsetY);
        set
        {
            TitleOffsetX = value.X;
            TitleOffsetY = value.Y;
        }
    }

    [NotMapped]
    public Offset2D LevelOffset
    {
        get => new(LevelOffsetX, LevelOffsetY);
        set
        {
            LevelOffsetX = value.X;
            LevelOffsetY = value.Y;
        }
    }

    [NotMapped]
    public Offset2D PfpOffset
    {
        get => new(PfpOffsetX, PfpOffsetY);
        set
        {
            PfpOffsetX = value.X;
            PfpOffsetY = value.Y;
        }
    }

    public UserRankcardConfig()
    {
    }

    public UserRankcardConfig(IUser user) => Id = user.Id;

    public UserRankcardConfig(ulong userId) => Id = userId;
}
