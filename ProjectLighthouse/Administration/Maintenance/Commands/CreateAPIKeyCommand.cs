using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Maintenance;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.Commands
{
    public class CreateApiKeyCommand : ICommand
    {
        public string Name() => "Create API Key";
        public string[] Aliases() => new[] { "createAPIKey", };
        public string Arguments() => "<description>";
        public int RequiredArgs() => 1;

        public async Task Run(string[] args, Logger logger)
        {
            ApiKeyEntity key = new() { Description = args[0], };
            if (string.IsNullOrWhiteSpace(key.Description))
            {
                key.Description = "<no description specified>";
            }
            key.Key = CryptoHelper.GenerateAuthToken();
            key.Created = DateTime.Now;
            DatabaseContext database = DatabaseContext.CreateNewInstance();
            await database.APIKeys.AddAsync(key);
            await database.SaveChangesAsync();
            logger.LogSuccess($"The API key has been created (id: {key.Id}), however for security the token will only be shown once.", LogArea.Command);
            logger.LogInfo($"Key: {key.Key}", LogArea.Command);
        }
    }
}

