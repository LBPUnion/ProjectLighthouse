using System;
using System.Collections.Generic;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class LabelHelper
{

    private static readonly List<string> lbp3Labels = new()
    {
        "LABEL_SINGLE_PLAYER",
        "LABEL_RPG",
        "LABEL_TOP_DOWN",
        "LABEL_CO_OP",
        "LABEL_1st_Person",
        "LABEL_3rd_Person",
        "LABEL_Sci_Fi",
        "LABEL_Social",
        "LABEL_Arcade_Game",
        "LABEL_Board_Game",
        "LABEL_Card_Game",
        "LABEL_Mini_Game",
        "LABEL_Party_Game",
        "LABEL_Defence",
        "LABEL_Driving",
        "LABEL_Hangout",
        "LABEL_Hide_And_Seek",
        "LABEL_Prop_Hunt",
        "LABEL_Music_Gallery",
        "LABEL_Costume_Gallery",
        "LABEL_Sticker_Gallery",
        "LABEL_Movie",
        "LABEL_Pinball",
        "LABEL_Technology",
        "LABEL_Homage",
        "LABEL_8_Bit",
        "LABEL_16_Bit",
        "LABEL_Seasonal",
        "LABEL_Time_Trial",
        "LABEL_INTERACTIVE_STREAM",
        "LABEL_QUESTS",
        "LABEL_SACKPOCKET",
        "LABEL_SPRINGINATOR",
        "LABEL_HOVERBOARD_NAME",
        "LABEL_FLOATY_FLUID_NAME",
        "LABEL_ODDSOCK",
        "LABEL_TOGGLE",
        "LABEL_SWOOP",
        "LABEL_SACKBOY",
        "LABEL_CREATED_CHARACTERS",
    };

    private static readonly Dictionary<string, string> translationTable = new()
    {
        {"Label_SinglePlayer", "Single Player"},
        {"LABEL_Quick", "Short"},
        {"LABEL_Competitive", "Versus"},
        {"LABEL_Puzzle", "Puzzler"},
        {"LABEL_Platform", "Platformer"},
        {"LABEL_Race", "Racer"},
        {"LABEL_SurvivalChallenge", "Survival Challenge"},
        {"LABEL_DirectControl", "Controllinator"},
        {"LABEL_GrapplingHook", "Grappling Hook"},
        {"LABEL_JumpPads", "Bounce Pads"},
        {"LABEL_MagicBag", "Creatinator"},
        {"LABEL_LowGravity", "Low Gravity"},
        {"LABEL_PowerGlove", "Grabinators"},
        {"LABEL_ATTRACT_GEL", "Attract-o-Gel"},
        {"LABEL_ATTRACT_TWEAK", "Attract-o-Tweaker"},
        {"LABEL_HEROCAPE", "Hero Cape"},
        {"LABEL_MEMORISER", "Memorizer"},
        {"LABEL_WALLJUMP", "Wall Jump"},
        {"LABEL_SINGLE_PLAYER", "Single Player"},
        {"LABEL_TOP_DOWN", "Top Down"},
        {"LABEL_CO_OP", "Co-Op"},
        {"LABEL_Sci_Fi", "Sci-Fi"},
        {"LABEL_INTERACTIVE_STREAM", "Interactive Stream"},
        {"LABEL_QUESTS", "Quests"},
        {"LABEL_Mini_Game", "Mini-Game"},
        {"8_Bit", "8-bit"},
        {"16_Bit", "16-bit"},
        {"LABEL_SACKPOCKET", "Sackpocket"},
        {"LABEL_SPRINGINATOR", "Springinator"},
        {"LABEL_HOVERBOARD_NAME", "Hoverboard"},
        {"LABEL_FLOATY_FLUID_NAME", "Floaty Fluid"},
        {"LABEL_ODDSOCK", "Oddsock"},
        {"LABEL_TOGGLE", "Toggle"},
        {"LABEL_SWOOP", "Swoop"},
        {"LABEL_SACKBOY", "Sackboy"},
        {"LABEL_CREATED_CHARACTERS", "Created Characters"},
    };

    public static bool isValidForGame(string label, GameVersion gameVersion)
    {
        return gameVersion switch
        {
            GameVersion.LittleBigPlanet1 => IsValidTag(label),
            GameVersion.LittleBigPlanet2 => IsValidLabel(label) && !lbp3Labels.Contains(label),
            GameVersion.LittleBigPlanet3 => IsValidLabel(label),
            _ => false,
        };
    }

    public static bool IsValidTag(string tag) => Enum.IsDefined(typeof(LevelTags), tag.Replace("TAG_", "").Replace("-", "_"));

    public static bool IsValidLabel(string label) => Enum.IsDefined(typeof(LevelLabels), label);

    public static string RemoveInvalidLabels(string authorLabels)
    {
        List<string> labels = new(authorLabels.Split(","));
        if (labels.Count > 5) labels = labels.GetRange(0, 5);

        for (int i = labels.Count - 1; i >= 0; i--)
        {
            if (!IsValidLabel(labels[i])) labels.Remove(labels[i]);
        }
        return string.Join(",", labels);
    }

    public static string TranslateTag(string tag)
    {
        if (tag.Contains("TAG_")) return tag.Replace("TAG_", "").Replace("_", "-");

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (translationTable.ContainsKey(tag)) return translationTable[tag];

        return tag.Replace("LABEL_", "").Replace("_", " ");
    }

}