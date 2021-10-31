using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Profiles;
using LBPUnion.ProjectLighthouse.Types.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse
{
    public class Database : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Slot> Slots { get; set; }
        public DbSet<QueuedLevel> QueuedLevels { get; set; }
        public DbSet<HeartedLevel> HeartedLevels { get; set; }
        public DbSet<HeartedProfile> HeartedProfiles { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Token> Tokens { get; set; }

        public DbSet<LastMatch> LastMatches { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseMySql(ServerSettings.DbConnectionString, MySqlServerVersion.LatestSupportedServerVersion);

        public async Task<User> CreateUser(string username)
        {
            User user;
            if ((user = await this.Users.Where(u => u.Username == username).FirstOrDefaultAsync()) != null) return user;

            Location l = new(); // store to get id after submitting
            this.Locations.Add(l); // add to table
            await this.SaveChangesAsync(); // saving to the database returns the id and sets it on this entity

            user = new User
            {
                Username = username,
                LocationId = l.Id,
                Biography = username + " hasn't introduced themselves yet.",
            };
            this.Users.Add(user);

            await this.SaveChangesAsync();

            return user;
        }

        #nullable enable
        public async Task<Token?> AuthenticateUser(LoginData loginData)
        {
            // TODO: don't use psn name to authenticate
            User user = await this.Users.FirstOrDefaultAsync(u => u.Username == loginData.Username) ?? await this.CreateUser(loginData.Username);

            Token token = new()
            {
                UserToken = HashHelper.GenerateAuthToken(),
                UserId = user.UserId,
            };

            this.Tokens.Add(token);
            await this.SaveChangesAsync();

            return token;
        }

        public async Task<User?> UserFromAuthToken(string authToken)
        {
            Token? token = await this.Tokens.FirstOrDefaultAsync(t => t.UserToken == authToken);
            if (token == null) return null;

            return await this.Users.Include(u => u.Location).FirstOrDefaultAsync(u => u.UserId == token.UserId);
        }

        public async Task<User?> UserFromRequest(HttpRequest request)
        {
            if (!request.Cookies.TryGetValue("MM_AUTH", out string? mmAuth) || mmAuth == null) return null;

            return await this.UserFromAuthToken(mmAuth);
        }
        #nullable disable
    }
}