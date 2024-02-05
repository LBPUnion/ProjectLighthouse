using System;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Filter.Sorts;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Filter.Sorts;

public class UniquePlaysForGameSort : ISlotSort
{
    private readonly GameVersion targetVersion;

    public UniquePlaysForGameSort(GameVersion targetVersion)
    {
        this.targetVersion = targetVersion;
    }

    private string GetColName() =>
        this.targetVersion switch
        {
            GameVersion.LittleBigPlanet1 => "LBP1",
            GameVersion.LittleBigPlanet2 => "LBP2",
            GameVersion.LittleBigPlanet3 => "LBP3",
            GameVersion.LittleBigPlanetVita => "LBP2",
            _ => "",
        };

    public Expression<Func<SlotEntity, dynamic>> GetExpression() => s => EF.Property<dynamic>(s, $"Plays{this.GetColName()}Unique");
}