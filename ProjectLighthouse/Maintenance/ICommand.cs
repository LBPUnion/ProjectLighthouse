using System.Threading.Tasks;

namespace LBPUnion.ProjectLighthouse.Maintenance;

public interface ICommand
{

    public string FirstAlias => this.Aliases()[0];
    public Task Run(string[] args);

    public string Name();

    public string[] Aliases();

    public string Arguments();

    public int RequiredArgs();
}