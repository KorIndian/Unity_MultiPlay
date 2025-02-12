using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.GameContents
{
	public class Item
	{
		public ItemInfo itemInfo { get; } = new ItemInfo();
		public int itemDbId
		{
			get => itemInfo.ItemDbId;
			set
			{
				itemInfo.ItemDbId = value;
			}
		}
		public int TemplateId
		{
			get => itemInfo.TemplateId;
			set
			{
				itemInfo.TemplateId = value;
			}
		}
		public int Count
		{
			get => itemInfo.Count;
			set => itemInfo.Count = value;
		}

		public int SlotNumber
		{
			get => itemInfo.SlotNumber;
			set => itemInfo.SlotNumber = value;
		}

		public ItemType ItemType { get; private set; }
		public bool Stackable { get; protected set; }

		public Item(ItemType itemType)
		{
			ItemType = itemType;
			if (ItemType == ItemType.Consumable)
			{
				Stackable = true;
			}
		}

		public static Item CreateItemByItemDb(ItemDb itemDb)
		{
			Item item = null;
			DataManager.ItemDict.TryGetValue(itemDb.TemplateId, out var itemData);

			switch (itemData.ItemType)
			{
				case ItemType.Weapon:
					item = new Weapon(itemDb.TemplateId);
					break;
				case ItemType.Armor:
					item = new Armor(itemDb.TemplateId);
					break;
				case ItemType.Consumable:
					item = new Consumable(itemDb.TemplateId);
					break;
				
			}

			if(item != null)
			{
				item.itemDbId = itemDb.ItemDbId;
				item.Count = itemDb.Count;
				item.SlotNumber = itemDb.SlotNumber;
			}

			return item;
		}
	}

	public class Weapon : Item
	{
		public WeaponType WeaponType { get; private set; }
		public int Damage { get; private set; }

		public Weapon(int templateId)
			: base(ItemType.Weapon)
		{
			Init(templateId);
		}

		void Init(int templateId)
		{
			DataManager.ItemDict.TryGetValue(templateId, out var itemData);
			if (itemData.ItemType != ItemType.Weapon)
			{
				Console.WriteLine("Wrong item type init.");
				return;
			}

			WeaponData data = (WeaponData)itemData;
			{
				TemplateId = data.Id;
				Count = 1;
				WeaponType = data.WeaponType;
				Damage = data.Damage;
				Stackable = false;
			}
		}
	}

	public class Armor : Item
	{
		public ArmorType ArmorType { get; private set; }
		public int Defence { get; private set; }

		public Armor(int templateId)
			: base(ItemType.Armor)
		{
			Init(templateId);
		}

		void Init(int templateId)
		{
			DataManager.ItemDict.TryGetValue(templateId, out var itemData);
			if (itemData.ItemType != ItemType.Armor)
			{
				Console.WriteLine("Wrong item type init.");
				return;
			}

			ArmorData data = (ArmorData)itemData;
			{
				TemplateId = data.Id;
				Count = 1;
				ArmorType = data.ArmorType;
				Defence = data.Defence;
				Stackable = false;
			}
		}
	}

	public class Consumable : Item
	{
		public ConsumableType ConsumableType { get; private set; }
		public int MaxCount { get; private set; }

		public Consumable(int templateId)
			: base(ItemType.Consumable)
		{
			Init(templateId);
		}

		void Init(int templateId)
		{
			DataManager.ItemDict.TryGetValue(templateId, out var itemData);
			if (itemData.ItemType != ItemType.Consumable)
			{
				Console.WriteLine("Wrong item type init.");
				return;
			}

			ConsumableData data = (ConsumableData)itemData;
			{
				TemplateId = data.Id;
				Count = 1;
				ConsumableType = data.ConsumableType;
				MaxCount = data.MaxCount;
				Stackable = (MaxCount > 1);
			}
		}
	}
}
