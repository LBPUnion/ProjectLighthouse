#nullable enable
using System.Threading.Tasks;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Pages
{
    public class SlotPage : BaseLayout
    {
        public SlotPage([NotNull] Database database) : base(database)
        {}

        public Slot Slot;

        public async Task<IActionResult> OnGet([FromRoute] int id)
        {
            Slot? slot = await this.Database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
            if (slot == null) return this.NotFound();

            this.Slot = slot;

            return this.Page();
        }
    }
}