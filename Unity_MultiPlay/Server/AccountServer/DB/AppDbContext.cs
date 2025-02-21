using Microsoft.EntityFrameworkCore;

namespace AccountServer.DB;

public class AppDbContext : DbContext
{
	public DbSet<AccountDb> Accounts { get; set; }

	public AppDbContext(DbContextOptions<AppDbContext> options)//asp.net이 configuring하는 방식
		: base(options)
	{

	}

	protected override void OnModelCreating(ModelBuilder Builder)
	{
		Builder.Entity<AccountDb>()
				.HasIndex(a => a.AccountName)
				.IsUnique();

	}
}
