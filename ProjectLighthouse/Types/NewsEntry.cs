using ProjectLighthouse.Serialization;

namespace ProjectLighthouse {
    public class NewsEntry {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Text { get; set; }
        public NewsImage Image { get; set; }
        public string Category { get; set; }
        public long Date { get; set; }

        public string Serialize() {
            return LbpSerializer.GetStringElement("id", Id) +
                   LbpSerializer.GetStringElement("title", Title) +
                   LbpSerializer.GetStringElement("summary", Summary) +
                   LbpSerializer.GetStringElement("text", Text) +
                   LbpSerializer.GetStringElement("date", Date) +
                   Image.Serialize() +
                   LbpSerializer.GetStringElement("category", Category);
        }
    }
}