using LBPUnion.ProjectLighthouse.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.News;

/// <summary>
///     Used on the info moon on LBP1. Broken for unknown reasons
/// </summary>
public static class News {
	public class NewsEntry
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Summary { get; set; }
		public string Text { get; set; }
		public NewsImage Image { get; set; }
		public long Date { get; set; }
		public string Type { get; set; }
		public string Name { get; set; }
		public string Author { get; set; }
		public string Icon { get; set; }
		public string Level { get; set; }
		public string Picks { get; set; }

		public string Serialize()
			=> LbpSerializer.StringElement("type", this.Type) + 
			LbpSerializer.StringElement("id", this.Id) +
			LbpSerializer.StringElement("picks", this.Picks) +
			LbpSerializer.StringElement("type", this.Type) +
			LbpSerializer.StringElement("date", this.Date) +
			LbpSerializer.StringElement("title", this.Title) +
			LbpSerializer.StringElement("summary", this.Summary) +
			LbpSerializer.StringElement("text", this.Text) +
			LbpSerializer.StringElement("date", this.Date) +
			this.Image.Serialize();
	}
	
	public class NewsImage
	{
		public string Hash { get; set; }
		public string Alignment { get; set; }
	
		public string Serialize()
			=> LbpSerializer.StringElement("image", LbpSerializer.StringElement("hash", this.Hash) + LbpSerializer.StringElement("alignment", this.Alignment));
	}
}