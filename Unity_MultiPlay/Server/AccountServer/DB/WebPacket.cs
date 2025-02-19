namespace AccountServer.DB;

public class CreateAccountPacketReq
{
	public required string AccountName { get; set; }
	public required string Password { get; set; }
}

public class CreateAccountPacketRes
{
	public bool CreateOk { get; set; }
}

public class LoginAccountPacketReq
{
	public string? AccountName { get; set; }
	public string? Password { get; set; }
}

public class ServerStatus
{
	public string? Name {  get; set; }
	public string? Ip { get; set; }
	public int CrowdedLevel { get; set; }
}


public class LoginAccountPacketRes
{
	public bool LoginOk { get; set; }
	public List<ServerStatus> ServerList { get; set; } = new List<ServerStatus>();
}