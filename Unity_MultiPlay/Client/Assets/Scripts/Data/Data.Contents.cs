using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class SkillData
    {
        public int id;
        public string name;
        public float cooldown;
        public int damage;
        public SkillType skillType;
        public ProjectileInfo projectileInfo;
    }

    [Serializable]
    public class ProjectileInfo
    {
        public string name;
        public float speed;
        public int range;
        public string prefabpath;
    }

    [Serializable]
    public class SkillDataLoader : ILoader<int, SkillData>
    {
        public List<SkillData> skillInfos = new List<SkillData>();

        public Dictionary<int, SkillData> MakeDict()
        {
            Dictionary<int, SkillData> dict = new Dictionary<int, SkillData>();
            foreach (SkillData skill in skillInfos)
                dict.Add(skill.id, skill);
            return dict;
        }
    }

	[Serializable]
	public class ItemData
	{
		public int Id;
		public string Name;// todo string id
		public ItemType ItemType;
		public string IconPath;
	}

	[Serializable]
	public class WeaponData : ItemData
	{
		public WeaponType WeaponType;
		public int Damage;
	}

	[Serializable]
	public class ArmorData : ItemData
	{
		public ArmorType ArmorType;
		public int Defence;
	}

	[Serializable]
	public class ConsumableData : ItemData
	{
		public ConsumableType ConsumableType;
		public int MaxCount;
	}

	[Serializable]
	public class ItemDataLoader : ILoader<int, ItemData>
	{
		//↓↓ Newtonsoft.Json.JsonConvert.DeserializeObject을 해주면 처음에 여기(stats) List형태로 들어와있음.
		public List<WeaponData> WeaponInfos = new List<WeaponData>();
		public List<ArmorData> ArmorInfos = new List<ArmorData>();
		public List<ConsumableData> ConsumableInfos = new List<ConsumableData>();

		public Dictionary<int, ItemData> MakeDict()
		{
			Dictionary<int, ItemData> dict = new Dictionary<int, ItemData>();
			foreach (ItemData itemInfo in WeaponInfos)
			{
				itemInfo.ItemType = ItemType.Weapon;
				dict.Add(itemInfo.Id, itemInfo);
			}
			foreach (ItemData itemInfo in ArmorInfos)
			{
				itemInfo.ItemType = ItemType.Armor;
				dict.Add(itemInfo.Id, itemInfo);
			}
			foreach (ItemData itemInfo in ConsumableInfos)
			{
				itemInfo.ItemType = ItemType.Consumable;
				dict.Add(itemInfo.Id, itemInfo);
			}
			return dict;
		}
	}

    #region Monster
    [Serializable]
    public class MonsterData
    {
        public int id;
        public string name;
        public string prefabPath;
        public StatInfo stat;
    }

    [Serializable]
    public class MonsterDataLoader : ILoader<int, MonsterData>
    {
        public List<MonsterData> MonsterDatas = new List<MonsterData>();

        public Dictionary<int, MonsterData> MakeDict()
        {
            Dictionary<int, MonsterData> dict = new Dictionary<int, MonsterData>();
            foreach (MonsterData monsterData in MonsterDatas)
            {
                dict.Add(monsterData.id, monsterData);
            }
            return dict;
        }
    }
    #endregion

}