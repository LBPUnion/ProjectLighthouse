using System.Threading.Tasks;

namespace LBPUnion.ProjectLighthouse.Maintenance;

public interface IMaintenanceJob
{
    public Task Run();

    public string Name();

    public string Description();
}