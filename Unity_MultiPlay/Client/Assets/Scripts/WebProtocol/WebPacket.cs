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

public class WebRequest
{
	public static void SendCreateAccount(string accountName, string password)
	{
		CreateAccountPacketReq req = new CreateAccountPacketReq()
		{ 
			AccountName = accountName,
			Password = password 
		};

		Managers.Web.SendPostRequest<CreateAccountPacketRes>("account/create", req, (res) =>
		{
			Debug.Log($"CreateAccount response : {res.CreateOk}");
		});
	}

	public static void SendLoginAccount(string accountName, string password)
	{
		LoginAccountPacketReq req = new LoginAccountPacketReq()
		{
			AccountName = accountName,
			Password = password
		};

		Managers.Web.SendPostRequest<LoginAccountPacketRes>("account/login", req, (res) =>
		{
			Debug.Log($"LoginAccount response : {res.LoginOk}");
		});
	}
}
