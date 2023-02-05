using System.Threading.Tasks;
using JetBrains.Annotations;

namespace LBPUnion.ProjectLighthouse.Types.Maintenance;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IMaintenanceJob
{
    public Task Run();

    public string Name();

    public string Description();
}