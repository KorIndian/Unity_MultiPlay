using Data;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory_Item : UI_Base
{
	[SerializeField]
	private Image itemIcon;

	public override void Init()
	{
	}

	public void SetItem(int templateId, int count)
    {
		DataManager.ItemDict.TryGetValue(templateId, out var itemData);
		if (itemData != null)
		{
			Sprite iconSprite = Managers.Resource.Load<Sprite>(itemData.IconPath);
			itemIcon.sprite = iconSprite;
			itemIcon.gameObject.SetActive(true);
		}
    }

	public void ClearItem()
	{
		itemIcon.sprite = null;
		itemIcon.gameObject.SetActive(false);
	}
}
