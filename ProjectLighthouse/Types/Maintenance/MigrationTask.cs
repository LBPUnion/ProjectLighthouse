using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;

namespace LBPUnion.ProjectLighthouse.Types.Maintenance;

public enum MigrationHook
{
    Before,
    None,
}

public abstract class MigrationTask
{
    /// <summary>
    /// The user-friendly name of a migration.
    /// </summary>
    public abstract string Name();

    public virtual MigrationHook HookType() => MigrationHook.None;

    /// <summary>
    /// Performs the migration.
    /// </summary>
    /// <param name="database">The Lighthouse database.</param>
    /// <returns>True if successful, false if not.</returns>
    public abstract Task<bool> Run(DatabaseContext database);
}