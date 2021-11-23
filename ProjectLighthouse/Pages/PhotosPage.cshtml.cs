using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Pages
{
    public class PhotosPage : BaseLayout
    {
        public PhotosPage([NotNull] Database database) : base(database)
        {}

        public int PhotoCount;

        public List<Photo> Photos;

        public int PageNumber;

        public async Task<IActionResult> OnGet([FromRoute] int pageNumber)
        {
            const int pageSize = 20;
            this.PhotoCount = await StatisticsHelper.PhotoCount();

            this.PageNumber = pageNumber;

            this.Photos = await this.Database.Photos.Include
                    (p => p.Creator)
                .OrderByDescending(p => p.Timestamp)
                .Skip(pageNumber * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return this.Page();
        }
    }
}