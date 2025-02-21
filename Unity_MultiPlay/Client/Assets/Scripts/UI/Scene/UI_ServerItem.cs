using CommonWebPacket;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_ServerItem : UI_Base
{
	[SerializeField]
	Text Txt_ServerName;

	[SerializeField]
	Text Txt_CrowdedLevel;

	public ServerStatus serverStatus { get; private set; }

	public override void AwakeInit()
	{
	}

	public void SetUI(ServerStatus _serverStatus)
	{
		serverStatus = _serverStatus;
		Txt_ServerName.text = serverStatus.Name;
		Txt_CrowdedLevel.text = $"Crowded Level : {serverStatus.CrowdedLevel}";
	}

	public void ClearItem()
	{
		serverStatus = null;
		Txt_ServerName.text = "";
		Txt_CrowdedLevel.text = "";
	}

	public void OnClickItem()
	{
		Managers.Network.ConnectToGameServer(serverStatus);
		SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
	}
}
