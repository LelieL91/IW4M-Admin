﻿using System;
using Data.Context;
using Data.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Data.MigrationContext
{
    public class PostgresqlDatabaseContext : DatabaseContext
    {
        public PostgresqlDatabaseContext()
        {
            if (!MigrationExtensions.IsMigration)
            {
                throw new InvalidOperationException();
            }
        }

        public PostgresqlDatabaseContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (MigrationExtensions.IsMigration)
            {
                optionsBuilder.UseNpgsql(
                        "Host=127.0.0.1;Database=IW4MAdmin_Migration;Username=postgres;Password=password;",
                        options => options.SetPostgresVersion(new Version("9.4")))
                    .EnableDetailedErrors(true)
                    .EnableSensitiveDataLogging(true);
            }
        }
    }
}