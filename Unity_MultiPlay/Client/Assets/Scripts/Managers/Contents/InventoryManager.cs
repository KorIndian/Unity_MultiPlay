using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//실질적으로 현제 클라이언트가 가지고있는 모든 아이템을 관리하는 매니저.
public class InventoryManager
{
	public UI_Inventory InventoryUI
	{
		get; private set;
	}

	public Dictionary<int, Item> Items
	{
		get => items;
		private set
		{
			items = value; 
		}
	}
	
	private Dictionary<int, Item> items = new Dictionary<int, Item>();

	public void Add(Item item)
	{
		items.Add(item.itemDbId, item);
	}

	public Item Get(int itemId)
	{
		items.TryGetValue(itemId, out var item);
		return item;
	}

	public Item Find(Func<Item, bool> condition)
	{
		return items.Values.ToList().Find(item => condition(item));
	}

	public void Clear()
	{
		items.Clear();
	}

	public void SetInventoryUI(UI_Inventory uI_Inventory)
	{
		InventoryUI = uI_Inventory;
	}
}
