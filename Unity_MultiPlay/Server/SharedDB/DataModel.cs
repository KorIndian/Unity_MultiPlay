using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedDB;

public class DataModel
{
	[Table("Token")]
	public class TokenDb
	{
		public int TokenDbId { get; set; }
		public int AccountDbId { get; set; }
		public string AccountName { get; set; } = string.Empty;
		public int Token { get; set; }
		public DateTime Expired { get; set; }
	}

	[Table("ServerInfo")]
	public class ServerStatusDb
	{
		public int ServerStatusDbId { get; set; }
		public string Name { get; set; } = string.Empty;
		public string IpAddress { get; set; } = string.Empty;
		public int Port { get; set; }
		public int CrowdedLevel { get; set; }

	}
}


