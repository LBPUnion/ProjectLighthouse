using System;
using System.Collections.Generic;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Filter.Filters;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Users;
using Xunit;

namespace LBPUnion.ProjectLighthouse.Tests.Unit;

[Trait("Category", "Unit")]
public class FilterTests
{
    [Fact]
    public void QueryBuilder_DoesDeepClone()
    {
        SlotQueryBuilder queryBuilder = new();
        queryBuilder.AddFilter(new CrossControlFilter());

        SlotQueryBuilder clonedBuilder = queryBuilder.Clone();

        Assert.NotEqual(queryBuilder, clonedBuilder);
    }

    [Fact]
    public void AdventureFilter_ShouldAccept_WhenAdventure()
    {
        AdventureFilter adventureFilter = new();
        Func<SlotEntity, bool> adventureFunc = adventureFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            IsAdventurePlanet = true,
        };

        Assert.True(adventureFunc(slot));
    }

    [Fact]
    public void AdventureFilter_ShouldReject_WhenNotAdventure()
    {
        AdventureFilter adventureFilter = new();
        Func<SlotEntity, bool> adventureFunc = adventureFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            IsAdventurePlanet = false,
        };

        Assert.False(adventureFunc(slot));
    }

    [Fact]
    public void AuthorLabelFilter_ShouldAccept_WhenExactMatch()
    {
        string[] filters =
        {
            "LABEL_Test", "LABEL_Unit",
        };
        AuthorLabelFilter labelFilter = new(filters);
        Func<SlotEntity, bool> labelFunc = labelFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            AuthorLabels = "LABEL_Test,LABEL_Unit",
        };

        Assert.True(labelFunc(slot));
    }

    [Fact]
    public void AuthorLabelFilter_ShouldAccept_WhenExactMatch_AndExtraLabelsPresent()
    {
        string[] filters =
        {
            "LABEL_Test", "LABEL_Unit",
        };
        AuthorLabelFilter labelFilter = new(filters);
        Func<SlotEntity, bool> labelFunc = labelFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            AuthorLabels = "LABEL_Test,LABEL_Unit,LABEL_Lighthouse,LABEL_Bruh",
        };

        Assert.True(labelFunc(slot));
    }

    [Fact]
    public void AuthorLabelFilter_ShouldAccept_WhenFilterEmpty_AndLabelsEmpty()
    {
        string[] filters = Array.Empty<string>();
        AuthorLabelFilter labelFilter = new(filters);
        Func<SlotEntity, bool> labelFunc = labelFilter.GetPredicate().Compile();

        SlotEntity slotWithNoLabels = new()
        {
            AuthorLabels = "",
        };

        Assert.True(labelFunc(slotWithNoLabels));
    }

    [Fact]
    public void AuthorLabelFilter_ShouldReject_WhenNoneMatch()
    {
        string[] filters =
        {
            "LABEL_Test", "LABEL_Unit",
        };
        AuthorLabelFilter labelFilter = new(filters);
        Func<SlotEntity, bool> labelFunc = labelFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            AuthorLabels = "LABEL_Adventure,LABEL_Versus",
        };

        Assert.False(labelFunc(slot));
    }

    [Fact]
    public void CreatorFilter_ShouldAccept_WhenCreatorIdMatch()
    {
        const int creatorId = 27;
        CreatorFilter creatorFilter = new(creatorId);
        Func<SlotEntity, bool> creatorFunc = creatorFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            CreatorId = creatorId,
        };

        Assert.True(creatorFunc(slot));
    }

    [Fact]
    public void CreatorFilter_ShouldReject_WhenCreatorIdMismatch()
    {
        const int filterCreatorId = 27;
        const int slotCreatorId = 28;
        CreatorFilter creatorFilter = new(filterCreatorId);
        Func<SlotEntity, bool> creatorFunc = creatorFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            CreatorId = slotCreatorId,
        };

        Assert.False(creatorFunc(slot));
    }

    [Fact]
    public void CrossControlFilter_ShouldAccept_WhenCrossControlRequired()
    {
        CrossControlFilter crossControlFilter = new();
        Func<SlotEntity, bool> ccFunc = crossControlFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            CrossControllerRequired = true,
        };

        Assert.True(ccFunc(slot));
    }

    [Fact]
    public void CrossControlFilter_ShouldReject_WhenCrossControlNotRequired()
    {
        CrossControlFilter crossControlFilter = new();
        Func<SlotEntity, bool> ccFunc = crossControlFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            CrossControllerRequired = false,
        };

        Assert.False(ccFunc(slot));
    }

    [Fact]
    public void ExcludeAdventureFilter_ShouldReject_WhenAdventure()
    {
        ExcludeAdventureFilter excludeAdventureFilter = new();
        Func<SlotEntity, bool> adventureFunc = excludeAdventureFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            IsAdventurePlanet = true,
        };

        Assert.False(adventureFunc(slot));
    }

    [Fact]
    public void ExcludeAdventureFilter_ShouldAccept_WhenNotAdventure()
    {
        ExcludeAdventureFilter excludeAdventureFilter = new();
        Func<SlotEntity, bool> adventureFunc = excludeAdventureFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            IsAdventurePlanet = false,
        };

        Assert.True(adventureFunc(slot));
    }

    [Fact]
    public void ExcludeCrossControlFilter_ShouldAccept_WhenNotCrossControl()
    {
        ExcludeCrossControlFilter crossControlFilter = new();
        Func<SlotEntity, bool> crossControlFunc = crossControlFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            CrossControllerRequired = false,
        };

        Assert.True(crossControlFunc(slot));
    }

    [Fact]
    public void ExcludeCrossControlFilter_ShouldReject_WhenCrossControl()
    {
        ExcludeCrossControlFilter crossControlFilter = new();
        Func<SlotEntity, bool> crossControlFunc = crossControlFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            CrossControllerRequired = true,
        };

        Assert.False(crossControlFunc(slot));
    }

    [Fact]
    public void ExcludeLBP1OnlyFilter_ShouldReject_WhenLbp1Only_AndTokenNotLbp1_AndNotCreator()
    {
        ExcludeLBP1OnlyFilter excludeLBP1 = new(10, GameVersion.LittleBigPlanet2);
        Func<SlotEntity, bool> excludeFunc = excludeLBP1.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            Lbp1Only = true,
        };

        Assert.False(excludeFunc(slot));
    }

    [Fact]
    public void ExcludeLBP1OnlyFilter_ShouldAccept_WhenLbp1Only_AndTokenLbp1()
    {
        ExcludeLBP1OnlyFilter excludeLBP1 = new(10, GameVersion.LittleBigPlanet1);
        Func<SlotEntity, bool> excludeFunc = excludeLBP1.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            Lbp1Only = true,
        };

        Assert.True(excludeFunc(slot));
    }

    [Fact]
    public void ExcludeLBP1OnlyFilter_ShouldAccept_WhenLbp1Only_AndTokenNotLbp1_AndIsCreator()
    {
        ExcludeLBP1OnlyFilter excludeLBP1 = new(10, GameVersion.LittleBigPlanet2);
        Func<SlotEntity, bool> excludeFunc = excludeLBP1.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            CreatorId = 10,
            Lbp1Only = true,
        };

        Assert.True(excludeFunc(slot));
    }

    [Fact]
    public void ExcludeMovePackFilter_ShouldReject_WhenMoveRequired()
    {
        ExcludeMovePackFilter excludeMove = new();
        Func<SlotEntity, bool> excludeFunc = excludeMove.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            MoveRequired = true,
        };

        Assert.False(excludeFunc(slot));
    }

    [Fact]
    public void ExcludeMovePackFilter_ShouldAccept_WhenMoveNotRequired()
    {
        ExcludeMovePackFilter excludeMove = new();
        Func<SlotEntity, bool> excludeFunc = excludeMove.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            MoveRequired = false,
        };
        
        Assert.True(excludeFunc(slot));
    }

    [Fact]
    public void FirstUploadedFilter_ShouldReject_WhenOlderThanStartTime()
    {
        FirstUploadedFilter uploadFilter = new(1000);
        Func<SlotEntity, bool> uploadFunc = uploadFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            FirstUploaded = 999,
        };

        Assert.False(uploadFunc(slot));
    }

    [Fact]
    public void FirstUploadedFilter_ShouldAccept_WhenNewerThanStartTime()
    {
        FirstUploadedFilter uploadFilter = new(1000);
        Func<SlotEntity, bool> uploadFunc = uploadFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            FirstUploaded = 1001,
        };

        Assert.True(uploadFunc(slot));
    }

    [Fact]
    public void FirstUploadedFilter_ShouldReject_WhenOlderThanEndTime()
    {
        FirstUploadedFilter uploadFilter = new(0, 1000);
        Func<SlotEntity, bool> uploadFunc = uploadFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            FirstUploaded = 1001,
        };

        Assert.False(uploadFunc(slot));
    }

    [Fact]
    public void FirstUploadedFilter_ShouldAccept_WhenNewerThanEndTime()
    {
        FirstUploadedFilter uploadFilter = new(0, 1000);
        Func<SlotEntity, bool> uploadFunc = uploadFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            FirstUploaded = 999,
        };

        Assert.True(uploadFunc(slot));
    }

    [Fact]
    public void GameVersionFilter_ShouldAccept_WhenExact_AndEqual()
    {
        GameVersionFilter gameVersionFilter = new(GameVersion.LittleBigPlanet1, true);
        Func<SlotEntity, bool> versionFunc = gameVersionFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            GameVersion = GameVersion.LittleBigPlanet1,
        };

        Assert.True(versionFunc(slot));
    }

    [Fact]
    public void GameVersionFilter_ShouldReject_WhenExact_AndNotEqual()
    {
        GameVersionFilter gameVersionFilter = new(GameVersion.LittleBigPlanet2, true);
        Func<SlotEntity, bool> versionFunc = gameVersionFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            GameVersion = GameVersion.LittleBigPlanet1,
        };

        Assert.False(versionFunc(slot));
    }

    [Fact]
    public void GameVersionFilter_ShouldAccept_WhenNotExact_AndGreaterThan()
    {
        GameVersionFilter gameVersionFilter = new(GameVersion.LittleBigPlanet2);
        Func<SlotEntity, bool> versionFunc = gameVersionFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            GameVersion = GameVersion.LittleBigPlanet1,
        };

        Assert.True(versionFunc(slot));
    }

    [Fact]
    public void GameVersionFilter_ShouldAccept_WhenNotExact_AndEqual()
    {
        GameVersionFilter gameVersionFilter = new(GameVersion.LittleBigPlanet2);
        Func<SlotEntity, bool> versionFunc = gameVersionFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            GameVersion = GameVersion.LittleBigPlanet2,
        };

        Assert.True(versionFunc(slot));
    }

    [Fact]
    public void GameVersionFilter_ShouldReject_WhenNotExact_AndLessThan()
    {
        GameVersionFilter gameVersionFilter = new(GameVersion.LittleBigPlanet1);
        Func<SlotEntity, bool> versionFunc = gameVersionFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            GameVersion = GameVersion.LittleBigPlanet2,
        };

        Assert.False(versionFunc(slot));
    }

    [Fact]
    public void GameVersionFilter_ShouldReject_WhenVersionNotInList()
    {
        GameVersionListFilter gameVersionListFilter = new(GameVersion.LittleBigPlanet1, GameVersion.LittleBigPlanet2);
        Func<SlotEntity, bool> versionFunc = gameVersionListFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            GameVersion = GameVersion.LittleBigPlanet3,
        };

        Assert.False(versionFunc(slot));
    }

    [Fact]
    public void GameVersionFilter_ShouldAccept_WhenVersionIsInList()
    {
        GameVersionListFilter gameVersionListFilter = new(GameVersion.LittleBigPlanet1, GameVersion.LittleBigPlanet2);
        Func<SlotEntity, bool> versionFunc = gameVersionListFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            GameVersion = GameVersion.LittleBigPlanet1,
        };

        Assert.True(versionFunc(slot));
    }

    [Fact]
    public void HiddenSlotFilter_ShouldReject_WhenHidden()
    {
        HiddenSlotFilter hiddenSlotFilter = new();
        Func<SlotEntity, bool> hiddenFunc = hiddenSlotFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            Hidden = true,
        };

        Assert.False(hiddenFunc(slot));
    }

    [Fact]
    public void HiddenSlotFilter_ShouldAccept_WhenNotHidden()
    {
        HiddenSlotFilter hiddenSlotFilter = new();
        Func<SlotEntity, bool> hiddenFunc = hiddenSlotFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            Hidden = false,
        };

        Assert.True(hiddenFunc(slot));
    }

    [Fact]
    public void MoveFilter_ShouldAccept_WhenMoveRequired()
    {
        MovePackFilter movePackFilter = new();
        Func<SlotEntity, bool> moveFunc = movePackFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            MoveRequired = true,
        };

        Assert.True(moveFunc(slot));
    }

    [Fact]
    public void MoveFilter_ShouldReject_WhenMoveNotRequired()
    {
        MovePackFilter movePackFilter = new();
        Func<SlotEntity, bool> moveFunc = movePackFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            MoveRequired = false,
        };

        Assert.False(moveFunc(slot));
    }

    [Fact]
    public void PlayerCountFilter_ShouldReject_WhenHigherThanMaxPlayers()
    {
        PlayerCountFilter playerCountFilter = new(maxPlayers: 2);
        Func<SlotEntity, bool> countFunc = playerCountFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            MinimumPlayers = 1,
            MaximumPlayers = 4,
        };

        Assert.False(countFunc(slot));
    }

    [Fact]
    public void PlayerCountFilter_ShouldReject_WhenLowerThanMinPlayers()
    {
        PlayerCountFilter playerCountFilter = new(minPlayers: 2);
        Func<SlotEntity, bool> countFunc = playerCountFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            MinimumPlayers = 1,
            MaximumPlayers = 4,
        };

        Assert.False(countFunc(slot));
    }

    [Fact]
    public void PlayerCountFilter_ShouldAccept_WhenLowerThanMaxPlayers()
    {
        PlayerCountFilter playerCountFilter = new(maxPlayers: 3);
        Func<SlotEntity, bool> countFunc = playerCountFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            MinimumPlayers = 1,
            MaximumPlayers = 2,
        };

        Assert.True(countFunc(slot));
    }

    [Fact]
    public void PlayerCountFilter_ShouldAccept_WhenHigherThanMinPlayers()
    {
        PlayerCountFilter playerCountFilter = new(minPlayers: 2);
        Func<SlotEntity, bool> countFunc = playerCountFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            MinimumPlayers = 3,
            MaximumPlayers = 4,
        };

        Assert.True(countFunc(slot));
    }

    [Fact]
    public void ResultTypeFilter_ShouldReject_WhenSlotNotPresent()
    {
        ResultTypeFilter resultFilter = new();
        Func<SlotEntity, bool> resultFunc = resultFilter.GetPredicate().Compile();

        SlotEntity slot = new();

        Assert.False(resultFunc(slot));
    }

    [Fact]
    public void ResultTypeFilter_ShouldAccept_WhenSlotPresent()
    {
        ResultTypeFilter resultFilter = new("slot");
        Func<SlotEntity, bool> resultFunc = resultFilter.GetPredicate().Compile();

        SlotEntity slot = new();

        Assert.True(resultFunc(slot));
    }

    [Fact]
    public void SlotIdFilter_ShouldReject_WhenSlotIdNotPresent()
    {
        SlotIdFilter idFilter = new(new List<int>
        {
            2,
        });
        Func<SlotEntity, bool> idFunc = idFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            SlotId = 1,
        };

        Assert.False(idFunc(slot));
    }

    [Fact]
    public void SlotIdFilter_ShouldAccept_WhenSlotIdPresent()
    {
        SlotIdFilter idFilter = new(new List<int>
        {
            2,
        });
        Func<SlotEntity, bool> idFunc = idFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            SlotId = 2,
        };

        Assert.True(idFunc(slot));
    }

    [Fact]
    public void SlotTypeFilter_ShouldAccept_WhenSlotTypeMatches()
    {
        SlotTypeFilter slotTypeFilter = new(SlotType.User);
        Func<SlotEntity, bool> typeFunc = slotTypeFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            Type = SlotType.User,
        };

        Assert.True(typeFunc(slot));
    }

    [Fact]
    public void SlotTypeFilter_ShouldAccept_WhenSlotTypeDoesNotMatch()
    {
        SlotTypeFilter slotTypeFilter = new(SlotType.User);
        Func<SlotEntity, bool> typeFunc = slotTypeFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            Type = SlotType.Developer,
        };

        Assert.False(typeFunc(slot));
    }

    [Fact]
    public void SubLevelFilter_ShouldAccept_WhenUserIsCreator_AndNotSubLevel()
    {
        SubLevelFilter subLevelFilter = new(2);
        Func<SlotEntity, bool> subLevelFunc = subLevelFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            CreatorId = 2,
            SubLevel = false,
        };

        Assert.True(subLevelFunc(slot));
    }

    [Fact]
    public void SubLevelFilter_ShouldAccept_WhenUserIsCreator_AndSubLevel()
    {
        SubLevelFilter subLevelFilter = new(2);
        Func<SlotEntity, bool> subLevelFunc = subLevelFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            CreatorId = 2,
            SubLevel = true,
        };

        Assert.True(subLevelFunc(slot));
    }

    [Fact]
    public void SubLevelFilter_ShouldReject_WhenUserIsNotCreator_AndSubLevel()
    {
        SubLevelFilter subLevelFilter = new(2);
        Func<SlotEntity, bool> subLevelFunc = subLevelFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            CreatorId = 1,
            SubLevel = true,
        };

        Assert.False(subLevelFunc(slot));
    }

    [Fact]
    public void SubLevelFilter_ShouldAccept_WhenUserIsNotCreator_AndNotSubLevel()
    {
        SubLevelFilter subLevelFilter = new(2);
        Func<SlotEntity, bool> subLevelFunc = subLevelFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            CreatorId = 1,
            SubLevel = false,
        };

        Assert.True(subLevelFunc(slot));
    }

    [Fact]
    public void TeamPickFilter_ShouldAccept_WhenTeamPick()
    {
        TeamPickFilter teamPickFilter = new();
        Func<SlotEntity, bool> teamPickFunc = teamPickFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            TeamPickTime = 1,
        };

        Assert.True(teamPickFunc(slot));
    }

    [Fact]
    public void TeamPickFilter_ShouldReject_WhenNotTeamPick()
    {
        TeamPickFilter teamPickFilter = new();
        Func<SlotEntity, bool> teamPickFunc = teamPickFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            TeamPickTime = 0,
        };

        Assert.False(teamPickFunc(slot));
    }

    [Fact]
    public void TextFilter_ShouldAccept_WhenDescriptionContainsText()
    {
        TextFilter textFilter = new("test");
        Func<SlotEntity, bool> textFunc = textFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            Description = "unit test",
        };

        Assert.True(textFunc(slot));
    }

    [Fact]
    public void TextFilter_ShouldReject_WhenDescriptionDoesNotContainText()
    {
        TextFilter textFilter = new("test");
        Func<SlotEntity, bool> textFunc = textFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            Description = "fraction exam",
        };

        Assert.False(textFunc(slot));
    }

    [Fact]
    public void TextFilter_ShouldAccept_WhenNameContainsText()
    {
        TextFilter textFilter = new("test");
        Func<SlotEntity, bool> textFunc = textFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            Name = "unit test",
        };

        Assert.True(textFunc(slot));
    }

    [Fact]
    public void TextFilter_ShouldReject_WhenNameDoesNotContainText()
    {
        TextFilter textFilter = new("test");
        Func<SlotEntity, bool> textFunc = textFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            Name = "fraction exam",
        };

        Assert.False(textFunc(slot));
    }

    [Fact]
    public void TextFilter_ShouldAccept_WhenIdContainsText()
    {
        TextFilter textFilter = new("21");
        Func<SlotEntity, bool> textFunc = textFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            SlotId = 21,
        };

        Assert.True(textFunc(slot));
    }

    [Fact]
    public void TextFilter_ShouldReject_WhenIdDoesNotContainText()
    {
        TextFilter textFilter = new("21");
        Func<SlotEntity, bool> textFunc = textFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            SlotId = 19,
        };

        Assert.False(textFunc(slot));
    }

    [Fact]
    public void TextFilter_ShouldAccept_WhenCreatorUsernameContainsText()
    {
        TextFilter textFilter = new("test");
        Func<SlotEntity, bool> textFunc = textFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            Creator = new UserEntity
            {
                Username = "test",
            },
        };

        Assert.True(textFunc(slot));
    }

    [Fact]
    public void TextFilter_ShouldReject_WhenCreatorUsernameDoesNotContainText()
    {
        TextFilter textFilter = new("test");
        Func<SlotEntity, bool> textFunc = textFilter.GetPredicate().Compile();

        SlotEntity slot = new()
        {
            Creator = new UserEntity
            {
                Username = "bruh",
            },
        };

        Assert.False(textFunc(slot));
    }
}