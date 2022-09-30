#nullable enable
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.Levels.Categories;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Slots;

[ApiController]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class CollectionController : ControllerBase
{
    private readonly Database database;

    public CollectionController(Database database)
    {
        this.database = database;
    }

    [HttpGet("playlists/{playlistId:int}/slots")]
    public async Task<IActionResult> GetPlaylistSlots(int playlistId)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        Playlist? targetPlaylist = await this.database.Playlists.FirstOrDefaultAsync(p => p.PlaylistId == playlistId);
        if (targetPlaylist == null) return this.BadRequest();

        IQueryable<Slot> slots = this.database.Slots.Include(s => s.Creator)
            .Include(s => s.Location)
            .Where(s => targetPlaylist.SlotIds.Contains(s.SlotId));

        string response = Enumerable.Aggregate(slots, string.Empty, (current, slot) => current + slot.Serialize());
        int total = targetPlaylist.SlotIds.Length;

        return this.Ok(LbpSerializer.TaggedStringElement("slots", response, "total", total));
    }

    [HttpPost("playlists/{playlistId:int}")]
    [HttpPost("playlists/{playlistId:int}/slots")]
    [HttpPost("playlists/{playlistId:int}/slots/{slotId:int}/delete")]
    [HttpPost("playlists/{playlistId:int}/order_slots")]
    public async Task<IActionResult> UpdatePlaylist(int playlistId, int slotId)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        Playlist? targetPlaylist = await this.database.Playlists.FirstOrDefaultAsync(p => p.PlaylistId == playlistId);
        if (targetPlaylist == null) return this.BadRequest();

        if (token.UserId != targetPlaylist.CreatorId) return this.BadRequest();

        // Delete a slot from playlist
        if (slotId != 0)
        {
            targetPlaylist.SlotIds = targetPlaylist.SlotIds.Where(s => s != slotId).ToArray();
            await this.database.SaveChangesAsync();
            return this.Ok(this.GetUserPlaylists(token.UserId));
        }

        Playlist? newPlaylist = await this.getPlaylistFromBody();

        if (newPlaylist == null) return this.BadRequest();

        if (newPlaylist.LevelIds != null)
        {
            HashSet<int> slotIds = new(targetPlaylist.SlotIds);

            // Reorder
            if (slotIds.SetEquals(newPlaylist.LevelIds))
            {
                targetPlaylist.SlotIds = newPlaylist.LevelIds;
            }
            // Add a level
            else
            {
                foreach (int id in newPlaylist.LevelIds)
                {
                    targetPlaylist.SlotIds = targetPlaylist.SlotIds.Append(id).ToArray();
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(newPlaylist.Name)) targetPlaylist.Name = newPlaylist.Name;

        if (!string.IsNullOrWhiteSpace(newPlaylist.Description)) targetPlaylist.Description = newPlaylist.Description;

        await this.database.SaveChangesAsync();

        return this.Ok(this.GetUserPlaylists(token.UserId));
    }

    private string GetUserPlaylists(int userId)
    {
        string response = Enumerable.Aggregate(
            this.database.Playlists.Include(p => p.Creator).Where(p => p.CreatorId == userId),
            string.Empty,
            (current, slot) => current + slot.Serialize());
        int total = this.database.Playlists.Count(p => p.CreatorId == userId);

        return LbpSerializer.TaggedStringElement("playlists", response, new Dictionary<string, object>
        {
            {"total", total},
            {"hint_start", total+1},
        });
    }

    [HttpPost("playlists")]
    public async Task<IActionResult> CreatePlaylist()
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        int playlistCount = await this.database.Playlists.CountAsync(p => p.CreatorId == token.UserId);

        if (playlistCount > ServerConfiguration.Instance.UserGeneratedContentLimits.ListsQuota) return this.BadRequest();

        Playlist? playlist = await this.getPlaylistFromBody();

        if (playlist == null) return this.BadRequest();

        playlist.CreatorId = token.UserId;

        this.database.Playlists.Add(playlist);

        await this.database.SaveChangesAsync();

        return this.Ok(this.GetUserPlaylists(token.UserId));
    }

    [HttpGet("user/{username}/playlists")]
    public async Task<IActionResult> GetUserPlaylists(string username)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        int targetUserId = await this.database.Users.Where(u => u.Username == username).Select(u => u.UserId).FirstOrDefaultAsync();
        if (targetUserId == 0) return this.BadRequest();

        return this.Ok(this.GetUserPlaylists(targetUserId));
    }

    [HttpGet("searches")]
    [HttpGet("genres")]
    public async Task<IActionResult> GenresAndSearches()
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        string categoriesSerialized = CategoryHelper.Categories.Aggregate
        (
            string.Empty,
            (current, category) =>
            {
                string serialized;

                if (category is CategoryWithUser categoryWithUser) serialized = categoryWithUser.Serialize(this.database, user);
                else serialized = category.Serialize(this.database);

                return current + serialized;
            }
        );

        categoriesSerialized += LbpSerializer.StringElement("text_search", LbpSerializer.StringElement("url", "/slots/searchLBP3"));

        return this.Ok
        (
            LbpSerializer.TaggedStringElement
            (
                "categories",
                categoriesSerialized,
                new Dictionary<string, object>
                {
                    {
                        "hint", ""
                    },
                    {
                        "hint_start", 1
                    },
                    {
                        "total", CategoryHelper.Categories.Count
                    },
                }
            )
        );
    }

    [HttpGet("searches/{endpointName}")]
    public async Task<IActionResult> GetCategorySlots(string endpointName, [FromQuery] int pageStart, [FromQuery] int pageSize)
    {
        (User, GameToken)? userAndToken = await this.database.UserAndGameTokenFromRequest(this.Request);

        if (userAndToken == null) return this.StatusCode(403, "");

        // ReSharper disable once PossibleInvalidOperationException
        User user = userAndToken.Value.Item1;
        GameToken gameToken = userAndToken.Value.Item2;

        Category? category = CategoryHelper.Categories.FirstOrDefault(c => c.Endpoint == endpointName);
        if (category == null) return this.NotFound();

        Logger.Debug("Found category " + category, LogArea.Category);

        List<Slot> slots;
        int totalSlots;

        if (category is CategoryWithUser categoryWithUser)
        {
            slots = categoryWithUser.GetSlots(this.database, user, pageStart, pageSize).ToList();
            totalSlots = categoryWithUser.GetTotalSlots(this.database, user);
        }
        else
        {
            slots = category.GetSlots(this.database, pageStart, pageSize).ToList();
            totalSlots = category.GetTotalSlots(this.database);
        }

        string slotsSerialized = slots.Aggregate(string.Empty, (current, slot) => current + slot.Serialize(gameToken.GameVersion));

        return this.Ok
        (
            LbpSerializer.TaggedStringElement
            (
                "results",
                slotsSerialized,
                new Dictionary<string, object>
                {
                    {
                        "total", totalSlots
                    },
                    {
                        "hint_start", pageStart + pageSize
                    },
                }
            )
        );
    }

    private async Task<Playlist?> getPlaylistFromBody()
    {
        this.Request.Body.Position = 0;
        string bodyString = await new StreamReader(this.Request.Body).ReadToEndAsync();

        string rootElement = bodyString.StartsWith("<playlist>") ? "playlist" : "levels";
        XmlSerializer serializer = new(typeof(Playlist), new XmlRootAttribute(rootElement));
        Playlist? playlist = (Playlist?)serializer.Deserialize(new StringReader(bodyString));

        SanitizationHelper.SanitizeStringsInClass(playlist);

        return playlist;
    }

}