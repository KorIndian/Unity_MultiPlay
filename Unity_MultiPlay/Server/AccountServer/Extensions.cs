using AccountServer.DB;
using SharedDB;
using System.Diagnostics;

namespace AccountServer
{
	public static class Extensions
	{
		public static bool SaveChangesEx(this AppDbContext db)
		{
			try
			{
				db.SaveChanges();
				return true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				return false;
			}
		}

		public static bool SaveChangesEx(this SharedDbContext db)
		{
			try
			{
				db.SaveChanges();
				return true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				return false;
			}
		}
	}
}
