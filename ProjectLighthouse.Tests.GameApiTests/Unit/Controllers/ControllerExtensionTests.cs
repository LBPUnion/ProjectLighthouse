using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Filter.Filters;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Slots;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Extensions;
using LBPUnion.ProjectLighthouse.Tests.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace ProjectLighthouse.Tests.GameApiTests.Unit.Controllers;

[Trait("Category", "Unit")]
public class ControllerExtensionTests
{
    [Fact]
    public void GetDefaultFilters_ShouldReturnFilterBuilder()
    {
        SlotQueryBuilder queryBuilder = new SlotsController(null!).GetDefaultFilters(MockHelper.GetUnitTestToken());

        Assert.NotEmpty(queryBuilder.GetFilters(typeof(GameVersionFilter)));
        Assert.NotEmpty(queryBuilder.GetFilters(typeof(HiddenSlotFilter)));
        Assert.NotEmpty(queryBuilder.GetFilters(typeof(SlotTypeFilter)));
    }

    [Fact]
    public void FilterFromRequest_ShouldAddExcludeLbp1Filter_WhenTokenNotLbp1()
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = GameVersion.LittleBigPlanet2;
        SlotsController controller = new(null!)
        {
            ControllerContext =
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString(),
                    },
                },
            },
        };

        SlotQueryBuilder queryBuilder = controller.FilterFromRequest(token);

        Assert.NotEmpty(queryBuilder.GetFilters(typeof(ExcludeLBP1OnlyFilter)));
    }

    [Fact]
    public void FilterFromRequest_ShouldReturnFilters_WhenQueryEmpty()
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = GameVersion.LittleBigPlanet2;
        SlotsController controller = new(null!)
        {
            ControllerContext =
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString(),
                    },
                },
            },
        };

        SlotQueryBuilder queryBuilder = controller.FilterFromRequest(token);

        Assert.NotEmpty(queryBuilder.GetFilters(typeof(GameVersionFilter)));
        Assert.NotEmpty(queryBuilder.GetFilters(typeof(ExcludeLBP1OnlyFilter)));
        Assert.NotEmpty(queryBuilder.GetFilters(typeof(SubLevelFilter)));
        Assert.NotEmpty(queryBuilder.GetFilters(typeof(HiddenSlotFilter)));
        Assert.NotEmpty(queryBuilder.GetFilters(typeof(SlotTypeFilter)));
    }

    [Fact]
    public void FilterFromRequest_ShouldAddLabelFilter_WhenAuthorLabelPresent()
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = GameVersion.LittleBigPlanet2;
        SlotsController controller = new(null!)
        {
            ControllerContext =
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?labelFilter0=LABEL_TEST"),
                    },
                },
            },
        };

        SlotQueryBuilder queryBuilder = controller.FilterFromRequest(token);
        Assert.NotEmpty(queryBuilder.GetFilters(typeof(AuthorLabelFilter)));
    }

    [Fact]
    public void FilterFromRequest_ShouldAddPlayerCountFilter_WhenPlayersPresent()
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = GameVersion.LittleBigPlanet2;
        SlotsController controller = new(null!)
        {
            ControllerContext =
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?players=1"),
                    },
                },
            },
        };

        SlotQueryBuilder queryBuilder = controller.FilterFromRequest(token);
        Assert.NotEmpty(queryBuilder.GetFilters(typeof(PlayerCountFilter)));
    }

    [Fact]
    public void FilterFromRequest_ShouldAddTextFilter_WhenTextFilterPresent()
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = GameVersion.LittleBigPlanet2;
        SlotsController controller = new(null!)
        {
            ControllerContext =
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?textFilter=test"),
                    },
                },
            },
        };

        SlotQueryBuilder queryBuilder = controller.FilterFromRequest(token);
        Assert.NotEmpty(queryBuilder.GetFilters(typeof(TextFilter)));
    }

    [Fact]
    public void FilterFromRequest_ShouldAddFirstUploadedFilter_WhenDateFilterPresent()
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = GameVersion.LittleBigPlanet2;
        SlotsController controller = new(null!)
        {
            ControllerContext =
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?dateFilterType=thisWeek"),
                    },
                },
            },
        };

        SlotQueryBuilder queryBuilder = controller.FilterFromRequest(token);
        Assert.NotEmpty(queryBuilder.GetFilters(typeof(FirstUploadedFilter)));
    }

    [Fact]
    public void FilterFromRequest_ShouldNotAddFirstUploadedFilter_WhenDateFilterInvalid()
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = GameVersion.LittleBigPlanet2;
        SlotsController controller = new(null!)
        {
            ControllerContext =
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?dateFilterType=thisMillenium"),
                    },
                },
            },
        };

        SlotQueryBuilder queryBuilder = controller.FilterFromRequest(token);
        Assert.Empty(queryBuilder.GetFilters(typeof(FirstUploadedFilter)));
    }

    [Fact]
    public void FilterFromRequest_ShouldAddExcludeMoveFilter_WhenMoveEqualsFalse()
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = GameVersion.LittleBigPlanet2;
        SlotsController controller = new(null!)
        {
            ControllerContext =
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?move=false"),
                    },
                },
            },
        };

        SlotQueryBuilder queryBuilder = controller.FilterFromRequest(token);
        Assert.NotEmpty(queryBuilder.GetFilters(typeof(ExcludeMovePackFilter)));
    }

    [Fact]
    public void FilterFromRequest_ShouldAddMoveFilter_WhenMoveEqualsOnly()
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = GameVersion.LittleBigPlanet2;
        SlotsController controller = new(null!)
        {
            ControllerContext =
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?move=only"),
                    },
                },
            },
        };

        SlotQueryBuilder queryBuilder = controller.FilterFromRequest(token);
        Assert.NotEmpty(queryBuilder.GetFilters(typeof(MovePackFilter)));
    }

    [Fact]
    public void FilterFromRequest_ShouldAddCrossControlFilter_WhenCrossControlEqualsTrue()
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = GameVersion.LittleBigPlanet2;
        SlotsController controller = new(null!)
        {
            ControllerContext =
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?crosscontrol=true"),
                    },
                },
            },
        };

        SlotQueryBuilder queryBuilder = controller.FilterFromRequest(token);
        Assert.NotEmpty(queryBuilder.GetFilters(typeof(CrossControlFilter)));
    }

    [Fact]
    public void FilterFromRequest_ShouldAddExcludeCrossControlFilter_WhenCrossControlNotTrue()
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = GameVersion.LittleBigPlanet2;
        SlotsController controller = new(null!)
        {
            ControllerContext =
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?crosscontrol=false"),
                    },
                },
            },
        };

        SlotQueryBuilder queryBuilder = controller.FilterFromRequest(token);
        Assert.NotEmpty(queryBuilder.GetFilters(typeof(ExcludeCrossControlFilter)));
    }

    [Fact]
    public void FilterFromRequest_ShouldAddExcludeCrossControlFilter_WhenCrossControlMissing()
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = GameVersion.LittleBigPlanet2;
        SlotsController controller = new(null!)
        {
            ControllerContext =
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString(),
                    },
                },
            },
        };

        SlotQueryBuilder queryBuilder = controller.FilterFromRequest(token);
        Assert.NotEmpty(queryBuilder.GetFilters(typeof(ExcludeCrossControlFilter)));
    }

    [Fact]
    public void FilterFromRequest_ShouldAddAdventureFilter_WhenAdventureEqualsAllMust()
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = GameVersion.LittleBigPlanet3;
        SlotsController controller = new(null!)
        {
            ControllerContext =
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?adventure=allMust"),
                    },
                },
            },
        };

        SlotQueryBuilder queryBuilder = controller.FilterFromRequest(token);
        Assert.NotEmpty(queryBuilder.GetFilters(typeof(AdventureFilter)));
    }

    [Fact]
    public void FilterFromRequest_ShouldAddExcludeAdventureFilter_WhenAdventureEqualsNoneCan()
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = GameVersion.LittleBigPlanet3;
        SlotsController controller = new(null!)
        {
            ControllerContext =
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?adventure=noneCan"),
                    },
                },
            },
        };

        SlotQueryBuilder queryBuilder = controller.FilterFromRequest(token);
        Assert.NotEmpty(queryBuilder.GetFilters(typeof(ExcludeAdventureFilter)));
    }

    [Fact]
    public void FilterFromRequest_ShouldAddMovePackFilter_WhenMoveEqualsAllMust()
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = GameVersion.LittleBigPlanet3;
        SlotsController controller = new(null!)
        {
            ControllerContext =
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?move=allMust"),
                    },
                },
            },
        };

        SlotQueryBuilder queryBuilder = controller.FilterFromRequest(token);
        Assert.NotEmpty(queryBuilder.GetFilters(typeof(MovePackFilter)));
    }

    [Fact]
    public void FilterFromRequest_ShouldAddExcludeMoveFilter_WhenMoveEqualsNoneCan()
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = GameVersion.LittleBigPlanet3;
        SlotsController controller = new(null!)
        {
            ControllerContext =
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?move=noneCan"),
                    },
                },
            },
        };

        SlotQueryBuilder queryBuilder = controller.FilterFromRequest(token);
        Assert.NotEmpty(queryBuilder.GetFilters(typeof(ExcludeMovePackFilter)));
    }

    [Fact]
    public void FilterFromRequest_ShouldAddGameVersionListFilter_WhenGameFilterIsPresent()
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = GameVersion.LittleBigPlanet3;
        SlotsController controller = new(null!)
        {
            ControllerContext =
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?gameFilter[]=lbp1&gameFilter[]=lbp3"),
                    },
                },
            },
        };

        SlotQueryBuilder queryBuilder = controller.FilterFromRequest(token);
        Assert.NotEmpty(queryBuilder.GetFilters(typeof(GameVersionListFilter)));
    }

    [Fact]
    public void FilterFromRequest_ShouldAddResultTypeFilter_WhenResultTypeIsPresent()
    {
        GameTokenEntity token = MockHelper.GetUnitTestToken();
        token.GameVersion = GameVersion.LittleBigPlanet3;
        SlotsController controller = new(null!)
        {
            ControllerContext =
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?resultType[]=slot&resultType[]=playlist"),
                    },
                },
            },
        };

        SlotQueryBuilder queryBuilder = controller.FilterFromRequest(token);
        Assert.NotEmpty(queryBuilder.GetFilters(typeof(ResultTypeFilter)));
    }
}