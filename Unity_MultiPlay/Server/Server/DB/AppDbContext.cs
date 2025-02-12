using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DB
{
	public class AppDbContext : DbContext
	{
		public DbSet<AccountDb> Accounts { get; set; }
		public DbSet<PlayerDb> Players {  get; set; }
		public DbSet<ItemDb> Items { get; set; }

		public string ConnectionString
		{
			get => connectionString;
			private set { connectionString = value; }
		}

		private string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=GameDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

		static readonly ILoggerFactory _loggerFactory = LoggerFactory.Create( builder =>
		{
			builder.AddConsole();
		});

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			string connectStr = ConfigManager.Config == null ?  ConnectionString : ConfigManager.Config.connectionString;

			optionsBuilder
				.UseLoggerFactory(_loggerFactory)
				.UseSqlServer(connectStr);
		}

		protected override void OnModelCreating(ModelBuilder Builder)
		{
			Builder.Entity<AccountDb>()
				.HasIndex(a => a.AccountName)
				.IsUnique();

			Builder.Entity<PlayerDb>()
				.HasIndex(p =>p.PlayerName)
				.IsUnique();
		}
	}
}
