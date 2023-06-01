using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Filter.Sorts;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Extensions;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Filter;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Misc;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Slots;

[ApiController]
[Authorize]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class CategoryController : ControllerBase
{
    private readonly DatabaseContext database;

    public CategoryController(DatabaseContext database)
    {
        this.database = database;
    }

    [HttpGet("searches")]
    [HttpGet("genres")]
    public async Task<IActionResult> GenresAndSearches()
    {
        GameTokenEntity token = this.GetToken();

        UserEntity? user = await this.database.UserFromGameToken(token);
        if (user == null) return this.Forbid();

        PaginationData pageData = this.Request.GetPaginationData();

        pageData.TotalElements = CategoryHelper.Categories.Count(c => !string.IsNullOrWhiteSpace(c.Name));

        if (!int.TryParse(this.Request.Query["num_categories_with_results"], out int results)) results = 5;

        List<GameCategory> categories = new();

        SlotQueryBuilder queryBuilder = this.FilterFromRequest(token);

        foreach (Category category in CategoryHelper.Categories.Where(c => !string.IsNullOrWhiteSpace(c.Name))
                     .Skip(Math.Max(0, pageData.PageStart - 1))
                     .Take(Math.Min(pageData.PageSize, pageData.MaxElements))
                     .ToList())
        {
            int numResults = results > 0 ? 1 : 0;
            categories.Add(await category.Serialize(this.database, token, queryBuilder, numResults));
            results--;
        }

        Category searchCategory = CategoryHelper.Categories.First(c => c.Tag == "text");
        GameCategory gameSearchCategory = GameCategory.CreateFromEntity(searchCategory, null);

        return this.Ok(new CategoryListResponse(categories, gameSearchCategory, pageData.TotalElements, "", pageData.HintStart));
    }

    [HttpGet("searches/{endpointName}")]
    public async Task<IActionResult> GetCategorySlots(string endpointName)
    {
        GameTokenEntity token = this.GetToken();

        UserEntity? user = await this.database.UserFromGameToken(token);
        if (user == null) return this.Forbid();

        Category? category = CategoryHelper.Categories.FirstOrDefault(c => c.Endpoint == endpointName);
        if (category == null) return this.NotFound();

        PaginationData pageData = this.Request.GetPaginationData();

        Logger.Debug("Found category " + category, LogArea.Category);

        SlotQueryBuilder queryBuilder = this.FilterFromRequest(token);

        GenericSerializableList returnList = category switch
        {
            SlotCategory gc => await this.GetSlotCategory(gc, token, queryBuilder, pageData),
            PlaylistCategory pc => await this.GetPlaylistCategory(pc, token, pageData),
            UserCategory uc => await this.GetUserCategory(uc, token, pageData),
            _ => new GenericSerializableList(),
        };

        return this.Ok(returnList);
    }

    private async Task<GenericSerializableList> GetUserCategory(UserCategory userCategory, GameTokenEntity token, PaginationData pageData)
    {
        int totalUsers = await userCategory.GetItems(this.database, token).CountAsync();
        pageData.TotalElements = totalUsers;
        IQueryable<UserEntity> userQuery = userCategory.GetItems(this.database, token).ApplyPagination(pageData);

        List<ILbpSerializable> users =
            (await userQuery.ToListAsync()).ToSerializableList<UserEntity, ILbpSerializable>(GameUser
                .CreateFromEntity);
        return new GenericSerializableList(users, pageData);
    }

    private async Task<GenericSerializableList> GetPlaylistCategory(PlaylistCategory playlistCategory, GameTokenEntity token, PaginationData pageData)
    {
        int totalPlaylists = await playlistCategory.GetItems(this.database, token).CountAsync();
        pageData.TotalElements = totalPlaylists;
        IQueryable<PlaylistEntity> playlistQuery = playlistCategory.GetItems(this.database, token).ApplyPagination(pageData);

        List<ILbpSerializable> playlists =
            (await playlistQuery.ToListAsync()).ToSerializableList<PlaylistEntity, ILbpSerializable>(GamePlaylist
                .CreateFromEntity);
        return new GenericSerializableList(playlists, pageData);
    }

    private async Task<GenericSerializableList> GetSlotCategory(SlotCategory slotCategory, GameTokenEntity token, SlotQueryBuilder queryBuilder, PaginationData pageData)
    {
        int totalSlots = await slotCategory.GetItems(this.database, token, queryBuilder).CountAsync();
        pageData.TotalElements = totalSlots;
        IQueryable<SlotEntity> slotQuery = slotCategory.GetItems(this.database, token, queryBuilder).ApplyPagination(pageData);

        if (bool.TryParse(this.Request.Query["includePlayed"], out bool includePlayed) && !includePlayed)
        {
            slotQuery = slotQuery.Select(s => new SlotMetadata
                {
                    Slot = s,
                    Played = this.database.VisitedLevels.Any(v => v.SlotId == s.SlotId && v.UserId == token.UserId),
                })
                .Where(s => !s.Played)
                .Select(s => s.Slot);
        }

        if (this.Request.Query.ContainsKey("sort"))
        {
            string sort = (string?)this.Request.Query["sort"] ?? "";
            slotQuery = sort switch
            {
                "relevance" => slotQuery.ApplyOrdering(new SlotSortBuilder<SlotEntity>()
                    .AddSort(new UniquePlaysTotalSort())
                    .AddSort(new LastUpdatedSort())),
                "likes" => slotQuery.Select(s => new SlotMetadata
                    {
                        Slot = s,
                        ThumbsUp = this.database.RatedLevels.Count(r => r.SlotId == s.SlotId && r.Rating == 1),
                    })
                    .OrderByDescending(s => s.ThumbsUp)
                    .Select(s => s.Slot),
                "hearts" => slotQuery.Select(s => new SlotMetadata
                    {
                        Slot = s,
                        Hearts = this.database.HeartedLevels.Count(h => h.SlotId == s.SlotId),
                    })
                    .OrderByDescending(s => s.Hearts)
                    .Select(s => s.Slot),
                "date" => slotQuery.ApplyOrdering(new SlotSortBuilder<SlotEntity>().AddSort(new FirstUploadedSort())),
                "plays" => slotQuery.ApplyOrdering(
                    new SlotSortBuilder<SlotEntity>().AddSort(new UniquePlaysTotalSort()).AddSort(new TotalPlaysSort())),
                _ => slotQuery,
            };
        }

        List<ILbpSerializable> slots =
            (await slotQuery.ToListAsync()).ToSerializableList<SlotEntity, ILbpSerializable>(s =>
                SlotBase.CreateFromEntity(s, token));
        return new GenericSerializableList(slots, pageData);
    }
}