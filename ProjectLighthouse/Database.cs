#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectLighthouse.Types;

namespace ProjectLighthouse {
    public class Database : DbContext {
        public DbSet<User> Users { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Slot> Slots { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Token> Tokens { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseMySql(
            ServerSettings.DbConnectionString,
            MySqlServerVersion.LatestSupportedServerVersion
        );
        
        public async Task<User> CreateUser(string username) {
            Location l = new(); // store to get id after submitting
            this.Locations.Add(l); // add to table
            await this.SaveChangesAsync(); // saving to the database returns the id and sets it on this entity

            User user = new() {
                Username = username,
                LocationId = l.Id,
                Biography = "No biography provided"
            };
            this.Users.Add(user);

            await this.SaveChangesAsync();

            return user;

        }

        // MM_AUTH=psn_name:?:timestamp, potentially a user creation date?:?:user id?:user's IP:?:password? SHA1
        // just blindly trust the token for now while we get it working
        public async Task<bool> AuthenticateUser(string mmAuth) {
            Token token = new() {
                MMAuth = mmAuth
            };

            string[] split;
            try {
                split = mmAuth.Split(":");
            }
            catch(ArgumentOutOfRangeException e) {
                return false; // Token doesn't contain :, cant be a valid token
            }
            
            // TODO: don't use psn name to authenticate
            User user = await this.Users.FirstOrDefaultAsync(u => u.Username == split[0]) 
                        ?? await this.CreateUser(split[0]);

            token.UserId = user.UserId;

            return true;
        }

        public async Task<bool> IsUserAuthenticated(string mmAuth) => await UserFromMMAuth(mmAuth) != null;

        public async Task<User?> UserFromMMAuth(string mmAuth) {
            Token? token = await Tokens.FirstOrDefaultAsync(t => t.MMAuth == mmAuth);
            if(token == null) return null;
            return await Users.FirstOrDefaultAsync(u => u.UserId == token.UserId);
        }
    }
}