using System.Collections.Generic;
using UnityEngine;

public class UI_GameScene : UI_Scene
{
	public UI_Stat StatUI { get; private set; }
	public UI_Inventory InvenUI { get; private set; }

	public override void AwakeInit()
	{
		base.AwakeInit();
		InvenUI = GetComponentInChildren<UI_Inventory>(true);
		InvenUI.gameObject.SetActive(false);

		StatUI = GetComponentInChildren<UI_Stat>(true);
		StatUI.gameObject.SetActive(false);

	}
}
