using System.Collections.Generic;
using System.Linq;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Filter;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace LBPUnion.ProjectLighthouse.Tests.Unit;

[Trait("Category", "Unit")]
public class PaginationTests
{
    [Fact]
    public void GetPaginationData_IsReadFromQuery()
    {
        DefaultHttpContext defaultHttpContext = new()
        {
            Request =
            {
                Query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    {
                        "pageStart", new StringValues("10")
                    },
                    {
                        "pageSize", new StringValues("15")
                    },
                }),
            },
        };

        PaginationData pageData = defaultHttpContext.Request.GetPaginationData();

        const int expectedPageStart = 10;
        const int expectedPageSize = 15;

        Assert.Equal(expectedPageStart, pageData.PageStart);
        Assert.Equal(expectedPageSize, pageData.PageSize);
    }

    [Fact]
    public void GetPaginationData_IsPageStartSetToDefault_WhenMissing()
    {
        DefaultHttpContext defaultHttpContext = new()
        {
            Request =
            {
                Query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    {
                        "pageSize", new StringValues("15")
                    },
                }),
            },
        };
        PaginationData pageData = defaultHttpContext.Request.GetPaginationData();

        const int expectedPageStart = 0;
        const int expectedPageSize = 15;

        Assert.Equal(expectedPageStart, pageData.PageStart);
        Assert.Equal(expectedPageSize, pageData.PageSize);
    }

    [Fact]
    public void GetPaginationData_IsPageSizeSetToDefault_WhenMissing()
    {
        ServerConfiguration.Instance.UserGeneratedContentLimits.EntitledSlots = 50;

        DefaultHttpContext defaultHttpContext = new()
        {
            Request =
            {
                Query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    {
                        "pageStart", new StringValues("10")
                    },
                }),
            },
        };
        PaginationData pageData = defaultHttpContext.Request.GetPaginationData();

        const int expectedPageStart = 10;
        const int expectedPageSize = 50;

        Assert.Equal(expectedPageStart, pageData.PageStart);
        Assert.Equal(expectedPageSize, pageData.PageSize);
    }

    [Fact]
    public void GetPaginationData_NegativeValuesAreSetToZero()
    {
        ServerConfiguration.Instance.UserGeneratedContentLimits.EntitledSlots = 50;

        DefaultHttpContext defaultHttpContext = new()
        {
            Request =
            {
                Query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    {
                        "pageStart", new StringValues("-10")
                    },
                    {
                        "pageSize", new StringValues("-10")
                    },
                }),
            },
        };
        PaginationData pageData = defaultHttpContext.Request.GetPaginationData();

        const int expectedPageStart = 0;
        const int expectedPageSize = 10;

        Assert.Equal(expectedPageStart, pageData.PageStart);
        Assert.Equal(expectedPageSize, pageData.PageSize);
    }

    [Fact]
    public void ApplyPagination_ShouldApplyCorrectPagination()
    {
        List<GameUser> users = new();
        for (int i = 0; i < 30; i++)
        {
            users.Add(new GameUser
            {
                UserId = i+1,
            });
        }

        PaginationData pageData = new()
        {
            PageSize = 5,
            PageStart = 6,
        };
        List<GameUser> pagedUsers = users.AsQueryable().ApplyPagination(pageData).ToList();

        Assert.Equal(pageData.PageSize, pagedUsers.Count);
        Assert.Equal(6, pagedUsers[0].UserId);
        Assert.Equal(10, pagedUsers[4].UserId);
    }

    [Fact]
    public void ApplyPagination_ShouldClampPageStart_WhenNegative()
    {
        List<GameUser> users = new();
        for (int i = 0; i < 30; i++)
        {
            users.Add(new GameUser
            {
                UserId = i + 1,
            });
        }

        PaginationData pageData = new()
        {
            PageSize = 5,
            PageStart = -5,
        };
        List<GameUser> pagedUsers = users.AsQueryable().ApplyPagination(pageData).ToList();

        Assert.Equal(pageData.PageSize, pagedUsers.Count);
        Assert.Equal(1, pagedUsers[0].UserId);
        Assert.Equal(5, pagedUsers[4].UserId);
    }

    [Fact]
    public void ApplyPagination_ShouldReturnEmpty_WhenPageSizeNegative()
    {
        List<GameUser> users = new();
        for (int i = 0; i < 30; i++)
        {
            users.Add(new GameUser
            {
                UserId = i + 1,
            });
        }

        PaginationData pageData = new()
        {
            PageSize = -5,
            PageStart = 0,
        };
        List<GameUser> pagedUsers = users.AsQueryable().ApplyPagination(pageData).ToList();

        Assert.Empty(pagedUsers);
    }

    [Fact]
    public void ApplyPagination_ShouldClampPageSize_WhenSizeExceedsMaxElements()
    {
        List<GameUser> users = new();
        for (int i = 0; i < 30; i++)
        {
            users.Add(new GameUser
            {
                UserId = i + 1,
            });
        }

        PaginationData pageData = new()
        {
            PageSize = 10,
            PageStart = 0,
            MaxElements = 1,
        };
        List<GameUser> pagedUsers = users.AsQueryable().ApplyPagination(pageData).ToList();

        Assert.Single(pagedUsers);
    }

    [Fact]
    public void ApplyPagination_ShouldClampPageSize_WhenSizeExceedsInternalLimit()
    {
        List<GameUser> users = new();
        for (int i = 0; i < 1001; i++)
        {
            users.Add(new GameUser
            {
                UserId = i + 1,
            });
        }

        PaginationData pageData = new()
        {
            PageSize = int.MaxValue,
            PageStart = 0,
            MaxElements = int.MaxValue,
        };
        List<GameUser> pagedUsers = users.AsQueryable().ApplyPagination(pageData).ToList();

        Assert.Equal(1000, pagedUsers.Count);
    }
}