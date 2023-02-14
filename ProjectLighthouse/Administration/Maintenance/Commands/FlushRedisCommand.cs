using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.StorableLists;
using LBPUnion.ProjectLighthouse.Types.Maintenance;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.Commands;

public class FlushRedisCommand : ICommand
{
    public string Name() => "Flush Redis";
    public string[] Aliases() => new[] {
        "flush", "flush-redis",
    };
    public string Arguments() => "";
    public int RequiredArgs() => 0;

    public async Task Run(string[] args, Logger logger)
    {
        await RedisDatabase.FlushAll();
    }
}