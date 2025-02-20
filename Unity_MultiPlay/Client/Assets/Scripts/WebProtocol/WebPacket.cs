using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateAccountPacketReq
{
	public string AccountName;
	public string Password;
}

public class CreateAccountPacketRes
{
	public bool CreateOk;
}

public class LoginAccountPacketReq
{
	public string AccountName;
	public string Password;
}

public class ServerStatus
{
	public string Name;
	public string Ip;
	public int CrowdedLevel;
}

public class LoginAccountPacketRes
{
	public bool LoginOk;
	public List<ServerStatus> ServerList = new List<ServerStatus>();
}


