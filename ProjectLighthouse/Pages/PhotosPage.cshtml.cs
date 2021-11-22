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

        public async Task<IActionResult> OnGet()
        {
            this.PhotoCount = await StatisticsHelper.PhotoCount();

            this.Photos = await this.Database.Photos.Include(p => p.Creator).Take(20).ToListAsync();

            return this.Page();
        }
    }
}