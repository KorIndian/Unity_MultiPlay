using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebRequest
{
	public static void SendCreateAccount(
		string accountName, 
		string password, 
		Action<CreateAccountPacketRes> callBack)
	{
		CreateAccountPacketReq req = new CreateAccountPacketReq()
		{
			AccountName = accountName,
			Password = password
		};

		Managers.Web.SendPostRequest<CreateAccountPacketRes>("account/create", req, callBack);
	}

	public static void SendLoginAccount(
		string accountName, 
		string password, 
		Action<LoginAccountPacketRes> callBack)
	{
		LoginAccountPacketReq req = new LoginAccountPacketReq()
		{
			AccountName = accountName,
			Password = password
		};

		Managers.Web.SendPostRequest<LoginAccountPacketRes>("account/login", req, callBack);
	}
}
