using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Data
{
    public interface ILoader<Key, Value>
    {
        Dictionary<Key, Value> MakeDict();
    }

    public static class DataManager
    {
        public static Dictionary<int, StatInfo> StatDict { get; private set; } = new Dictionary<int, StatInfo>();
        public static Dictionary<int, SkillData> SkillDict { get; private set; } = new Dictionary<int, SkillData>();
        public static Dictionary<int, ItemData> ItemDict { get; private set; } = new Dictionary<int, ItemData>();
        public static Dictionary<int, MonsterData> MonsterDict { get; private set; } = new Dictionary<int, MonsterData>();
        public static void LoadData()
        {
            StatDict = LoadCommonJson<StatDataLoader, int, StatInfo>("S_StatData").MakeDict();
            SkillDict = LoadCommonJson<SkillDataLoader, int, SkillData>("S_SkillData").MakeDict();
            ItemDict = LoadCommonJson<ItemDataLoader, int, ItemData>("S_ItemData").MakeDict();
            MonsterDict = LoadCommonJson<MonsterDataLoader, int, MonsterData>("S_MonsterData").MakeDict();
        }

		//이 함수는 Common/config.json 에 박혀있는 패스에서 데이터들을 읽어온다. 
		static LoaderType LoadCommonJson<LoaderType, Key, Value>(string fileName) where LoaderType : ILoader<Key, Value>
        {
            string JsonString = File.ReadAllText($"{ConfigManager.Config.dataPath}/{fileName}.json");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<LoaderType>(JsonString);
        }
    }
}
