using System;
using System.Collections.Generic;
using LBPUnion.ProjectLighthouse.Levels;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class LabelHelper
{

    private static readonly Dictionary<string, string> translationTable = new()
    {
        {"LABEL_DirectControl", "Controllinator"},
        {"LABEL_GrapplingHook", "Grappling Hook"},
        {"LABEL_JumpPads", "Bounce Pads"},
        {"LABEL_MagicBag", "Creatinator"},
        {"LABEL_LowGravity", "Low Gravity"},
        {"LABEL_PowerGlove", "Grabinator"},
        {"LABEL_ATTRACT_GEL", "Attract-o-Gel"},
        {"LABEL_ATTRACT_TWEAK", "Attract-o-Tweaker"},
        {"LABEL_HEROCAPE", "Hero Cape"},
        {"LABEL_MEMORISER", "Memorizer"},
        {"LABEL_WALLJUMP", "Wall Jump"},
        {"LABEL_SINGLE_PLAYER", "Single Player"},
        {"LABEL_SurvivalChallenge", "Survival Challenge"},
        {"LABEL_TOP_DOWN", "Top Down"},
        {"LABEL_CO_OP", "Co-Op"},
        {"LABEL_Sci_Fi", "Sci-Fi"},
        {"LABEL_INTERACTIVE_STREAM", "Interactive Stream"},
        {"LABEL_QUESTS", "Quests"},
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

    public static bool IsValidTag(string tag) => Enum.IsDefined(typeof(LevelTags), tag.Replace("TAG_", "").Replace("-", "_"));

    public static bool IsValidLabel(string label) => Enum.IsDefined(typeof(LevelLabels), label);

    public static string TranslateTag(string tag)
    {
        if (tag.Contains("TAG_")) return tag.Replace("TAG_", "");

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (translationTable.ContainsKey(tag)) return translationTable[tag];

        return tag.Replace("LABEL_", "").Replace("_", " ");
    }

}