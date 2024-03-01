namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class UseDigestAttribute : Attribute
{
    public bool EnforceDigest { get; set; } = true;

    public string DigestHeaderName { get; set; } = "X-Digest-A";

    public bool ExcludeBodyFromDigest { get; set; }
}