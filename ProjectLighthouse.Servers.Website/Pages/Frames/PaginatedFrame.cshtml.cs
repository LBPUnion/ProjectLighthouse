using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Filter;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Frames;

public abstract class PaginatedFrame : BaseFrame
{
    protected PaginatedFrame(DatabaseContext database) : base(database)
    { }

    public int CurrentPage { get; set; }
    public int TotalItems { get; set; }
    public int ItemsPerPage { get; set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(this.TotalItems / (float)this.ItemsPerPage));

    public PaginationData PageData =>
        new()
        {
            MaxElements = this.ItemsPerPage,
            PageSize = this.ItemsPerPage,
            PageStart = (this.CurrentPage - 1) * this.ItemsPerPage + 1,
            TotalElements = this.TotalItems,
        };

    /// <summary>
    /// Only call after setting CurrentPage and TotalItems
    /// </summary>
    public void ClampPage() => this.CurrentPage = Math.Clamp(this.CurrentPage, 1, this.TotalPages);
}