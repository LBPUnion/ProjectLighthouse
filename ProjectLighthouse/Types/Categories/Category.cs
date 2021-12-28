#nullable enable
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Types.Categories
{
    [XmlType("category")]
    [XmlRoot("category")]
    public abstract class Category
    {
        [XmlElement("name")]
        public abstract string Name { get; set; }

        [XmlElement("description")]
        public abstract string Description { get; set; }

        [XmlElement("icon")]
        public abstract string IconHash { get; set; }

        [XmlIgnore]
        public abstract string Endpoint { get; set; }

        [XmlElement("url")]
        public string IngameEndpoint {
            get => $"/searches/{this.Endpoint}";
            set => this.Endpoint = value.Replace("/searches/", "");
        }

        public abstract Slot? GetPreviewSlot(Database database);

        public string Serialize(Database database)
        {
            Slot? previewSlot = this.GetPreviewSlot(database);

            string previewResults = "";
            if (previewSlot != null)
            {
                previewResults = LbpSerializer.StringElement("results", LbpSerializer.StringElement("slots", previewSlot.Serialize()));
            }

            return LbpSerializer.StringElement
            (
                "category",
                LbpSerializer.StringElement("name", this.Name) +
                LbpSerializer.StringElement("description", this.Description) +
                LbpSerializer.StringElement("url", this.IngameEndpoint) +
                (previewSlot == null ? "" : previewResults) +
                LbpSerializer.StringElement("icon", IconHash)
            );
        }
    }
}