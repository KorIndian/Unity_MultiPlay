using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Stat : UI_Scene
{
	enum Images
	{
		Slot_Helmet,
		Slot_Weapon,
		Slot_Armor,
		Slot_Boots,
		Slot_Shield
	}
	enum Texts
	{
		txt_Name,
		txt_Attack,
		txt_Defence,
		txt_MaxHP
	}

	private bool bInitialized = false;

	public override void AwakeInit()
	{
		if (bInitialized)
			return;

		base.AwakeInit();

		Bind<Image>(typeof(Images));
		Bind<Text>(typeof(Texts));

		bInitialized = true;
	}

	public void RefreshUI()
	{
		if (!bInitialized)
			AwakeInit();

		Get<Image>((int)Images.Slot_Helmet).enabled = false;
		Get<Image>((int)Images.Slot_Weapon).enabled = false;
		Get<Image>((int)Images.Slot_Armor).enabled = false;
		Get<Image>((int)Images.Slot_Boots).enabled = false;
		Get<Image>((int)Images.Slot_Shield).enabled = false;

		foreach (Item item in Managers.Inventory.Items.Values)
		{
			if (item.Equipped == false)
				continue;

			DataManager.ItemDict.TryGetValue(item.TemplateId, out var ItemData);
			Sprite icon = Managers.Resource.Load<Sprite>(ItemData.IconPath);

			if (item.ItemType == ItemType.Weapon)
			{
				Get<Image>((int)Images.Slot_Weapon).enabled = true;
				Get<Image>((int)Images.Slot_Weapon).sprite = icon;
			}
			else if (item.ItemType == ItemType.Armor)
			{
				Armor armor = (Armor)item;
				switch (armor.ArmorType)
				{
					case ArmorType.Helmet:
						Get<Image>((int)Images.Slot_Helmet).enabled = true;
						Get<Image>((int)Images.Slot_Helmet).sprite = icon;
						break;
					case ArmorType.Armor:
						Get<Image>((int)Images.Slot_Armor).enabled = true;
						Get<Image>((int)Images.Slot_Armor).sprite = icon;
						break;
					case ArmorType.Boots:
						Get<Image>((int)Images.Slot_Boots).enabled = true;
						Get<Image>((int)Images.Slot_Boots).sprite = icon;
						break;
				}
			}
		}

		MyPlayerController myPC = Managers.Object.MyPlayer;
		myPC.ReCalcAdditionalStat();

		Get<Text>((int)Texts.txt_Name).text = myPC.name;
		Get<Text>((int)Texts.txt_Attack).text = $"{myPC.Stat.Attack}+{myPC.WeaponDamage}";
		Get<Text>((int)Texts.txt_Defence).text = $"{myPC.ArmorDefence}";
		Get<Text>((int)Texts.txt_MaxHP).text = $"{myPC.Stat.MaxHp}";
	}

	public override void ToggleUI()
	{
		RefreshUI();
		base.ToggleUI();
	}
}
