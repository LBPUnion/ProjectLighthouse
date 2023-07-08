using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Frames;

public class ScoreFrame : PaginatedFrame
{
    public List<(int Rank, ScoreEntity Score)> Scores = new();

    public int ScoreType { get; set; }

    public ScoreFrame(DatabaseContext database) : base(database)
    {
        this.ItemsPerPage = 10;
    }

    public async Task<IActionResult> OnGet([FromQuery] int page, int slotId, int? scoreType)
    {
        this.CurrentPage = page;
        SlotEntity? slot = await this.Database.Slots.FindAsync(slotId);
        if (slot == null) return this.BadRequest();

        scoreType ??= slot.LevelType switch
        {
            "versus" => 7,
            _ => 1,
        };

        Func<int?, bool> isValidFunc = slot.LevelType switch
        {
            "versus" => type => type == 7,
            _ => type => type is >= 1 and <= 4,
        };

        if (!isValidFunc(scoreType)) return this.BadRequest();

        IQueryable<ScoreEntity> scoreQuery = this.Database.Scores.Where(s => s.SlotId == slotId)
            .Where(s => s.Type == scoreType);

        this.TotalItems = await scoreQuery.CountAsync();

        this.ClampPage();

        this.Scores = (await scoreQuery.OrderByDescending(s => s.Points)
                .ThenBy(s => s.ScoreId)
                .Select(s => new
                {
                    Rank = scoreQuery.Count(s2 => s2.Points > s.Points) + 1,
                    Score = s,
                })
                .ApplyPagination(this.PageData)
                .ToListAsync()).Select(s => (s.Rank, s.Score))
            .ToList();

        return this.Page();
    }
}