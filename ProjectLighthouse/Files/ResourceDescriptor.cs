namespace LBPUnion.ProjectLighthouse.Files;

public class ResourceDescriptor
{
    public string Hash;
    public int Type;

    public bool IsScriptType() => this.Type == 0x11;

    public bool IsGuidResource() => this.Hash.StartsWith("g");
}