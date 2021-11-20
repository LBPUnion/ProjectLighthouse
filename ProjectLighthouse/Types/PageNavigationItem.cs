namespace LBPUnion.ProjectLighthouse.Types
{
    public class PageNavigationItem
    {
        public PageNavigationItem(string name, string url)
        {
            this.Name = name;
            this.Url = url;
        }
        public string Name { get; set; }
        public string Url { get; set; }
    }
}