using System.Threading.Tasks;
using JetBrains.Annotations;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IMigrationTask
{
    /// <summary>
    /// The user-friendly name of a migration.
    /// </summary>
    public string Name();
    
    /// <summary>
    /// Performs the migration.
    /// </summary>
    /// <param name="database">The Lighthouse database.</param>
    /// <returns>True if successful, false if not.</returns>
    internal Task<bool> Run(Database database);
}