using LBPUnion.ProjectLighthouse.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.News
{
    public class NewsImage
    {
        public string Hash { get; set; }
        public string Alignment { get; set; }

        public string Serialize()
            => LbpSerializer.StringElement("image", LbpSerializer.StringElement("hash", this.Hash) + LbpSerializer.StringElement("alignment", this.Alignment));
    }
}