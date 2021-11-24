using System.Threading.Tasks;

namespace LBPUnion.ProjectLighthouse.CommandLine
{
    public interface ICommand
    {
        public Task Run(string[] args);

        public string Name();

        public string[] Aliases();

        public string FirstAlias => this.Aliases()[0];

        public string Arguments();

        public int RequiredArgs();
    }
}