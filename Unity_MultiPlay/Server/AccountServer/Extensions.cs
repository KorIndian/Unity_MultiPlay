using AccountServer.DB;
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
	}
}
