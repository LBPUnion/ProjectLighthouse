using System;
using Microsoft.EntityFrameworkCore;
using ProjectLighthouse.Types;

namespace ProjectLighthouse {
    public class Database : DbContext {
        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseMySql(
            ServerSettings.DbConnectionString,
            MySqlServerVersion.LatestSupportedServerVersion
        );
    }
}