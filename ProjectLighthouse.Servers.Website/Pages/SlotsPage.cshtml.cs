#nullable enable
using System.Text;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class SlotsPage : BaseLayout
{

    public int PageAmount;

    public int PageNumber;

    public int SlotCount;

    public List<SlotEntity> Slots = new();

    public string? SearchValue;

    public SlotsPage(DatabaseContext database) : base(database)
    {}

    public async Task<IActionResult> OnGet([FromRoute] int pageNumber, [FromQuery] string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) name = "";

        string? targetAuthor = null;
        GameVersion? targetGame = null;
        StringBuilder finalSearch = new();
        foreach (string part in name.Split(" "))
        {
            if (part.Contains("by:"))
            {
                targetAuthor = part.Replace("by:", "");
            }
            else if (part.Contains("game:"))
            {
                if (part.Contains('1')) targetGame = GameVersion.LittleBigPlanet1;
                else if (part.Contains('2')) targetGame = GameVersion.LittleBigPlanet2;
                else if (part.Contains('3')) targetGame = GameVersion.LittleBigPlanet3;
                else if (part.Contains('v')) targetGame = GameVersion.LittleBigPlanetVita;
            }
            else
            {
                finalSearch.Append(part).Append(' ');
            }
        }

        this.SearchValue = name.Trim();
        
        string trimmedSearch = finalSearch.ToString().Trim();

        IQueryable<SlotEntity> slots = this.Database.Slots.Include(p => p.Creator)
            .Where(p => p.Type == SlotType.User && !p.Hidden)
            .Where(p => p.Name.Contains(trimmedSearch))
            .Where(p => p.Creator != null && (targetAuthor == null || string.Equals(p.Creator.Username.ToLower(), targetAuthor.ToLower())))
            .Where(p => p.Creator != null && (!p.SubLevel || p.Creator == this.User))
            .Where(p => targetGame == null || p.GameVersion == targetGame);

        this.SlotCount = await slots.CountAsync();

        this.PageNumber = pageNumber;
        this.PageAmount = Math.Max(1, (int)Math.Ceiling((double)this.SlotCount / ServerStatics.PageSize));

        if (this.PageNumber < 0 || this.PageNumber >= this.PageAmount) return this.Redirect($"/slots/{Math.Clamp(this.PageNumber, 0, this.PageAmount - 1)}");

        this.Slots = await slots
            .OrderByDescending(p => p.FirstUploaded)
            .Skip(pageNumber * ServerStatics.PageSize)
            .Take(ServerStatics.PageSize)
            .ToListAsync();

        return this.Page();
    }
}