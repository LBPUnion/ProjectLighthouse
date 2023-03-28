using System;
using System.ComponentModel.DataAnnotations;
using LBPUnion.ProjectLighthouse.Types.Maintenance;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Maintenance;

/// <summary>
/// A record of the completion of a <see cref="IMigrationTask"/>.
/// </summary>
public class CompletedMigrationEntity
{
    /// <summary>
    ///     The name of the migration.
    /// </summary>
    /// <remarks>
    ///     Do not use the user-friendly name when setting this.
    /// </remarks>
    [Key]
    public string MigrationName { get; set; }
    
    /// <summary>
    ///     The moment the migration was ran.
    /// </summary>
    public DateTime RanAt { get; set; }
}