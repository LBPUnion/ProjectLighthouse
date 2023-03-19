namespace LBPUnion.ProjectLighthouse.Types.Serialization;

/// <summary>
/// Used for serializable classes that benefit from having a custom root
/// <para>For example: If the underlying properties of a type don't change but the root tag does</para>
/// </summary>
public interface IHasCustomRoot
{
    public string GetRoot();
}