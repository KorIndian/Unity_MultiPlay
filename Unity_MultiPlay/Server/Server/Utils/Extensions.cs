using Server.DB;
using SharedDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
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
