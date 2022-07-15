using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.Helpers;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.Commands
{
    public class CreateAPIKeyCommand : ICommand
    {
        public string Name() => "Create API Key";
        public string[] Aliases()
            => new[]
            {
            "createAPIKey",
            };
        public string Arguments() => "<description>";
        public int RequiredArgs() => 1;

        public async Task Run(string[] args, Logger logger)
        {
            APIKey key = new();
            key.Description = args[0];
            if (string.IsNullOrWhiteSpace(key.Description))
            {
                key.Description = "<Blank>";
            }
            key.Key = CryptoHelper.GenerateAuthToken();
            key.Created = DateTime.Now;
            key.Enabled = true;
            Database database = new();
            await database.APIKeys.AddAsync(key);
            await database.SaveChangesAsync();
            logger.LogSuccess("API Key created, for security it will only be shown once", LogArea.Command);
            logger.LogInfo($"Key: {key.Key}", LogArea.Command);
        }
    }
}

