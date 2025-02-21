using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace CommonWebPacket
{
	public enum WebPacketID
	{
		QueryServerStatusReq = 0,
		QueryServerStatusRes,
		CreateAccountPacketReq,
		CreateAccountPacketRes,
		LoginAccountPacketReq,
		LoginAccountPacketRes,

	}

	public interface WebPacket
	{
		public ushort PacketId { get; set; }
	}

	public class QueryServerStatusReq : WebPacket
	{
		public ushort PacketId { get; set; } = (ushort)WebPacketID.QueryServerStatusReq;
		public string data { get; set; } = string.Empty;
	}

	public class QueryServerStatusRes : WebPacket
	{
		public ushort PacketId { get; set; } = (ushort)WebPacketID.QueryServerStatusRes;
		public string data { get; set; } = string.Empty;
	}

	public class CreateAccountPacketReq : WebPacket
	{
		public ushort PacketId { get; set; } = (ushort)WebPacketID.CreateAccountPacketReq;
		public string AccountName { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
	}

	public class CreateAccountPacketRes : WebPacket
	{
		public ushort PacketId { get; set; } = (ushort)WebPacketID.CreateAccountPacketRes;
		public bool CreateOk { get; set; }
	}

	public class LoginAccountPacketReq : WebPacket
	{
		public ushort PacketId { get; set; } = (ushort)WebPacketID.LoginAccountPacketReq;
		public string AccountName { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
	}

	public class LoginAccountPacketRes : WebPacket
	{
		public ushort PacketId { get; set; } = (ushort)WebPacketID.LoginAccountPacketRes;

		public bool LoginOk { get; set; }
		public int AccountId { get; set; }
		public string AccountName { get; set; } = string.Empty;
		public int Token { get; set; }
		public List<ServerStatus> ServerList { get; set; } = new List<ServerStatus>();
	}

	public class ServerStatus
	{
		public string Name { get; set; } = string.Empty;
		public string IpAddress { get; set; } = string.Empty;
		public int Port { get; set; }
		public int CrowdedLevel { get; set; }
	}
}

