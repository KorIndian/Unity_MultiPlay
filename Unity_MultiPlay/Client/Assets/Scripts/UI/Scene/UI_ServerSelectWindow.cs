using CommonWebPacket;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_ServerSelectWindow : UI_Base
{
	public static int MaxItemCount { get; private set; } = 8;
	
	public List<ServerStatus> ServerList {  get; private set; }
	public List<UI_ServerItem> ServerItems { get; private set; } = new List<UI_ServerItem>();

	[SerializeField]
	Transform GridTransform;

	public override void AwakeInit()
	{
		ClearItems();
	}

	public void RefreshServerUI(List<ServerStatus> serverList)
	{
		ServerList = serverList;

		if (ServerList.Count == 0)
			return;

		ClearItems();
		foreach (var serverStat in ServerList)
		{
			GameObject ItemGO = Managers.Resource.Instantiate("UI/Scene/UI_ServerItem", GridTransform);
			UI_ServerItem item = ItemGO.GetOrAddComponent<UI_ServerItem>();
			item.ClearItem();
			item.SetUI(serverStat);
			ServerItems.Add(item);
		}
	}

	public void OnClickCancel()
	{
		if (Managers.UI.SceneUI is UI_LoginScene loginSceneUI)
		{
			loginSceneUI.OpenLoginWindow();
		}
	}

	private void ClearItems()
	{
		ServerItems.Clear();
		foreach (Transform trans in GridTransform)
		{
			Destroy(trans.gameObject);
		}
	}

}
