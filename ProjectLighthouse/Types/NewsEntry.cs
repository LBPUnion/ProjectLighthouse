using ProjectLighthouse.Serialization;

namespace ProjectLighthouse.Types {
    public class NewsEntry {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Text { get; set; }
        public NewsImage Image { get; set; }
        public string Category { get; set; }
        public long Date { get; set; }

        public string Serialize() {
            return LbpSerializer.GetStringElement("id", this.Id) +
                   LbpSerializer.GetStringElement("title", this.Title) +
                   LbpSerializer.GetStringElement("summary", this.Summary) +
                   LbpSerializer.GetStringElement("text", this.Text) +
                   LbpSerializer.GetStringElement("date", this.Date) +
                   this.Image.Serialize() +
                   LbpSerializer.GetStringElement("category", this.Category);
        }
    }
}