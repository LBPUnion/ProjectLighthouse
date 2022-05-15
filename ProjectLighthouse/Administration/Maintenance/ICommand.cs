using System.Threading.Tasks;
using JetBrains.Annotations;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface ICommand
{

    public string FirstAlias => this.Aliases()[0];
    public Task Run(string[] args);

    public string Name();

    public string[] Aliases();

    public string Arguments();

    public int RequiredArgs();
}