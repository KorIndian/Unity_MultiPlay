using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;

public class UI_Inventory : UI_Scene
{
	public static int InventoryItemCount { get; private set; } = 54;
	public List<UI_Inventory_Item> ItemUIs { get; } = new List<UI_Inventory_Item>();

	public bool IsInit { get; private set; } = false;

	public override void Init()
	{
		if (IsInit)
			return;

		ItemUIs.Clear();

		GameObject grid = transform.Find("grid_Items").gameObject;

		foreach (Transform trans in grid.transform)
		{
			Destroy(trans.gameObject);
		}

		for (int i = 0; i < InventoryItemCount; ++i)
		{
			GameObject ItemGO = Managers.Resource.Instantiate("UI/Scene/UI_Inventory_Item", grid.transform);
			UI_Inventory_Item item = ItemGO.GetOrAddComponent<UI_Inventory_Item>();
			item.ClearItem();
			item.gameObject.SetActive(true);
			ItemUIs.Add(item);
		}

		IsInit = true;
	}

	public void RefreshUI()
	{
		if (IsInit == false)
			Init();

		var itemlist = Managers.Inventory.Items.Values.ToList();
		//itemlist.Sort((left, right) => left.SlotNumber - right.SlotNumber);
		itemlist.ForEach(item => 
		{
			if (item.SlotNumber < 0 || item.SlotNumber > InventoryItemCount)
				return;

			ItemUIs[item.SlotNumber].SetItem(item.TemplateId, item.Count);
			ItemUIs[item.SlotNumber].gameObject.SetActive(true);
		});

	}

	public override void ToggleUI()
	{
		if(gameObject.activeSelf)
			gameObject.SetActive(false);
		else 
			gameObject.SetActive(true);
	}

}
