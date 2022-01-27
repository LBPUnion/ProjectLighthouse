namespace LBPUnion.ProjectLighthouse.Types;

public class Version
{
    public int Major { get; set; }
    public int Minor { get; set; }

    public Version(int major, int minor)
    {
        this.Major = major;
        this.Minor = minor;
    }

    public override string ToString() => $"{this.Major}.{this.Minor}";

    public static implicit operator string(Version v) => v.ToString();
}