using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace LBPUnion.ProjectLighthouse.Types.Categories
{
    public class DatabaseCategory
    {
        [Key]
        public int CategoryId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string IconHash { get; set; }
        public string Endpoint { get; set; }

        public string SlotIdsCollection { get; set; }

        [NotMapped]
        public int[] SlotIds {
            get => SlotIdsCollection.Split(",").Select(int.Parse).ToArray();
            set => SlotIdsCollection = string.Join(",", value);
        }
    }
}