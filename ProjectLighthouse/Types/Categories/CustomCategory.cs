#nullable enable
using System.Collections.Generic;
using System.Linq;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Types.Categories
{
    public class CustomCategory : Category
    {
        public CustomCategory(string name, string description, string endpoint, string icon, IEnumerable<int> slotIds)
        {
            this.Name = name;
            this.Description = description;
            this.IconHash = icon;
            this.Endpoint = endpoint;

            this.SlotIds = slotIds.ToList();
        }

        public List<int> SlotIds;

        public sealed override string Name { get; set; }
        public sealed override string Description { get; set; }
        public sealed override string IconHash { get; set; }
        public sealed override string Endpoint { get; set; }
        public override Slot? GetPreviewSlot(Database database) => database.Slots.FirstOrDefault(s => s.SlotId == this.SlotIds[0]);
        public override IEnumerable<Slot> GetSlots(Database database, int pageStart, int pageSize)
            => database.Slots.Where(s => this.SlotIds.Contains(s.SlotId));
        public override int GetTotalSlots(Database database) => this.SlotIds.Count;
    }
}