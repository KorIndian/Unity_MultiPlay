using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SharedDB.DataModel;

namespace SharedDB;

public class SharedDbContext : DbContext
{
	public DbSet<TokenDb> Tokens { get; set; }
	public DbSet<ServerStatusDb> Servers { get; set; }

	public static string ConnectionString { get; set; } = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SharedDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

	public SharedDbContext()
	{

	}

	public SharedDbContext(DbContextOptions<SharedDbContext> options)//asp.net이 configuring하는 방식
		: base(options)
	{

	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)//일반 콘솔 어플리케이션 configuring 방식 
	{
		if(optionsBuilder.IsConfigured == false)
		{
			string connectStr = ConnectionString;

			optionsBuilder
				//.UseLoggerFactory(_loggerFactory)
				.UseSqlServer(connectStr);
		}
		
	}

	protected override void OnModelCreating(ModelBuilder Builder)
	{
		Builder.Entity<TokenDb>()
			.HasIndex(t => t.AccountDbId)
			.IsUnique();

		Builder.Entity<TokenDb>()
			.HasIndex(t => t.AccountName)
			.IsUnique();

		Builder.Entity<ServerStatusDb>()
			.HasIndex(s => s.Name)
			.IsUnique();
	}

}
