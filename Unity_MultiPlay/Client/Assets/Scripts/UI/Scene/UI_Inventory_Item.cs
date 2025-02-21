using Data;
using Google.Protobuf.Protocol;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory_Item : UI_Base
{
	[SerializeField]
	private Image itemIcon;

	[SerializeField]
	private Image itemFrame;

	public Item refItem { get; private set; }

	public int? ItemDbId => refItem?.itemDbId;
	public int? TemplateId => refItem?.TemplateId;
	public int? Count => refItem?.Count;
	public bool? Equipped => refItem?.Equipped;
	

	public override void AwakeInit()
	{
		itemIcon.gameObject.BindEvent(e =>
		{
			if (refItem == null)
				return;

			Debug.Log("Click Item");
			if (refItem.ItemType == ItemType.Consumable || refItem.ItemType == ItemType.None)
				return;

			refItem.Equipped = !refItem.Equipped;
			
			C_EquipItem equipItem = new C_EquipItem();

			equipItem.ItemDbId = ItemDbId.Value;
			equipItem.Equipped = refItem.Equipped;

			Managers.Network.Send(equipItem);
		});
	}

	public void SetItem(Item _item)
    {
		if (_item == null)
		{
			refItem = null;
			itemIcon.gameObject.SetActive(false);
			itemFrame.gameObject.SetActive(false);
			return;
		}
			
		refItem = _item;
		DataManager.ItemDict.TryGetValue(refItem.TemplateId, out var itemData);
		if (itemData != null)
		{
			Sprite iconSprite = Managers.Resource.Load<Sprite>(itemData.IconPath);
			itemIcon.sprite = iconSprite;
			itemIcon.gameObject.SetActive(true);
			itemFrame.gameObject.SetActive(refItem.Equipped);
		}
	}

	public void ClearItem()
	{
		refItem = null;
		itemIcon.sprite = null;
		itemIcon.gameObject.SetActive(false);
	}
}
