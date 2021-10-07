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
                "INSERT INTO Locations (X, Y) VALUES ({0}, {1})",
                0, 0);

            Location l = new() {
                X = 0,
                Y = 0
            };

            this.Locations.Add(l);
            await this.SaveChangesAsync();

            int locationId = l.Id;

            await this.Database.ExecuteSqlRawAsync(
                "INSERT INTO Users (Username, Biography, Pins, LocationId) VALUES ({0}, {1}, {2}, {3})",
                username, "No biography provided.", "", locationId);
            
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Slot> Slots { get; set; }
        public DbSet<Comment> Comments { get; set; }
    }
}