using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectLighthouse.Types;

namespace ProjectLighthouse {
    public class Database : DbContext {
        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseMySql(
            ServerSettings.DbConnectionString,
            MySqlServerVersion.LatestSupportedServerVersion
        );

        public async Task CreateUser(string username) {
            await this.Database.ExecuteSqlRawAsync(
                "INSERT INTO Users (Username, Biography) VALUES ({0}, {1})",
                username, "");
        }
        
        public DbSet<User> Users { get; set; }
    }
}