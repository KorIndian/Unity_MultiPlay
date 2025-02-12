using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
	public MyPlayerController MyPlayer { get; set; }
	Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();
	
	public static GameObjectType GetGameObjectTypeById(int id)
    {
		int type = (id >> 24) & 0x7F;
		return (GameObjectType)type;
    }

	public void AddObject(ObjectInfo info, bool bMyPlayer = false)
    {
		GameObjectType type = GetGameObjectTypeById(info.ObjectId);
		if(type == GameObjectType.Player)
        {
            if (bMyPlayer)//내 플레이어를 소환해야 하는 경우.
            {
                GameObject go = Managers.Resource.Instantiate("Creature/MyPlayer");
                go.name = info.Name;
                _objects.Add(info.ObjectId, go);//PlayerId는 서버가 발급하게 되어있다.

                MyPlayer = go.GetComponent<MyPlayerController>();
                MyPlayer.ObjectId = info.ObjectId;
                MyPlayer.PosInfo = info.PosInfo;
				MyPlayer.Stat = info.StatInfo;
                MyPlayer.SyncPos();
            }
            else//다른 유저플레이어를 소환해야하는 경우.
            {
                GameObject go = Managers.Resource.Instantiate("Creature/Player");
                go.name = info.Name;
                _objects.Add(info.ObjectId, go);

                PlayerController pc = go.GetComponent<PlayerController>();
                pc.ObjectId = info.ObjectId;
                pc.PosInfo = info.PosInfo;
				pc.Stat = info.StatInfo;
                pc.SyncPos();
            }
        }
		else if(type == GameObjectType.Monster)
        {
			DataManager.MonsterDict.TryGetValue(info.TemplateId, out var monsterdata);
            GameObject go = Managers.Resource.Instantiate(monsterdata.prefabPath);

            go.name = info.Name;
            _objects.Add(info.ObjectId, go);

			MonsterController mc = go.GetComponent<MonsterController>();
            mc.ObjectId = info.ObjectId;
            mc.PosInfo = info.PosInfo;
            mc.Stat = info.StatInfo;
            mc.SyncPos();
        }
        else if (type == GameObjectType.Projectile)
        {
			GameObject go = Managers.Resource.Instantiate("Creature/Arrow");
			
			go.name = "Arrow";
			_objects.Add(info.ObjectId, go);

			ArrowController ac = go.GetComponent<ArrowController>();
			ac.PosInfo = info.PosInfo;
			ac.Stat = info.StatInfo;
			ac.SyncPos();
        }
    }

	public void Add(int id, GameObject go)
	{
		_objects.Add(id,go);
	}

	public void Remove(int id)
	{
		GameObject go = FindById(id);
		if (go == null)
            return;

        _objects.Remove(id);
        Managers.Resource.Destroy(go);
    }

	public GameObject FindById(int id)
    {
		GameObject go = null;
		_objects.TryGetValue(id, out go);
		return go;
    }

	public GameObject FindCreature(Vector3Int cellPos)
	{
		foreach (GameObject obj in _objects.Values)
		{
			CreatureController cc = obj.GetComponent<CreatureController>();
			if (cc == null)
				continue;

			if (cc.CellPos == cellPos)
				return obj;
		}

		return null;
	}

	public GameObject FindCreature(Func<GameObject, bool> condition)
	{
		foreach (GameObject obj in _objects.Values)
		{
			if (condition.Invoke(obj))
				return obj;
		}

		return null;
	}

	public void Clear()
	{
		foreach (GameObject obj in _objects.Values)
		{
			Managers.Resource.Destroy(obj);
		}
		_objects.Clear();
		MyPlayer = null;
	}
}
