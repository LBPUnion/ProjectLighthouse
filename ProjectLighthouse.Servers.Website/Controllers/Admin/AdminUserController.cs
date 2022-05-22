#nullable enable
using LBPUnion.ProjectLighthouse.Administration;
using LBPUnion.ProjectLighthouse.Files;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IOFile = System.IO.File;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Controllers.Admin;

[ApiController]
[Route("admin/user/{id:int}")]
public class AdminUserController : ControllerBase
{
    private readonly Database database;

    public AdminUserController(Database database)
    {
        this.database = database;
    }

    [HttpGet("unban")]
    public async Task<IActionResult> UnbanUser([FromRoute] int id)
    {
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsAdmin) return this.NotFound();

        User? targetedUser = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (targetedUser == null) return this.NotFound();

        targetedUser.PermissionLevel = PermissionLevel.Default;
        targetedUser.BannedReason = null;

        await this.database.SaveChangesAsync();
        return this.Redirect($"/user/{targetedUser.UserId}");
    }
    
    /// <summary>
    /// Resets the user's earth decorations to a blank state. Useful for users who abuse audio for example.
    /// </summary>
    [HttpGet("wipePlanets")]
    public async Task<IActionResult> WipePlanets([FromRoute] int id) {
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsAdmin) return this.NotFound();

        User? targetedUser = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (targetedUser == null) return this.NotFound();
        
        string[] hashes = {
            targetedUser.PlanetHashLBP2,
            targetedUser.PlanetHashLBP3,
            targetedUser.PlanetHashLBPVita,
        };

        // This will also wipe users' earth with the same hashes.
        foreach (string hash in hashes)
        {
            // Don't try to remove empty hashes. That's a horrible idea.
            if (string.IsNullOrWhiteSpace(hash)) continue;
            
            // Find users with a matching hash
            List<User> users = await this.database.Users
                .Where(u => u.PlanetHashLBP2 == hash ||
                            u.PlanetHashLBP3 == hash ||
                            u.PlanetHashLBPVita == hash)
                .ToListAsync();

            // We should match at least the targeted user...
            System.Diagnostics.Debug.Assert(users.Count != 0);
            
            // Reset each users' hash.
            foreach (User userWithPlanet in users)
            {
                userWithPlanet.PlanetHashLBP2 = "";
                userWithPlanet.PlanetHashLBP3 = "";
                userWithPlanet.PlanetHashLBPVita = "";
                Logger.Success($"Deleted planets for {userWithPlanet.Username} (id:{userWithPlanet.UserId})", LogArea.Admin);
            }
            
            // And finally, attempt to remove the resource from the filesystem. We don't want that taking up space.
            try
            {
                IOFile.Delete(FileHelper.GetResourcePath(hash));
                Logger.Success($"Deleted planet resource {hash}",
                    LogArea.Admin);
            }
            catch(DirectoryNotFoundException)
            {
                // This is certainly a strange case, but it's not worth doing anything about since we were about
                // to delete the file anyways. Carry on~
            }
            catch(Exception e)
            {
                // Welp, guess I'll die then. We tried~
                Logger.Error($"Failed to delete planet resource {hash}\n{e}", LogArea.Admin);
            }
        }

        await this.database.SaveChangesAsync();

        return this.Redirect($"/user/{targetedUser.UserId}");
    }
}