﻿using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.GameContents;

public partial class GameRoom : JobSerializer
{
	public int RoomId { get; set; }
	Dictionary<int, Player> _players = new Dictionary<int, Player>();
	Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
	Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();

	public Map Map { get; private set; } = new Map();

	bool bRespawning = false;

	public void Init(int mapId)
	{
		Map.LoadMap(mapId, "../../../../../Common/MapData");
		//Temp
		Monster monster = ObjectManager.Instance.AddObject<Monster>();
		monster.Init(1);
		monster.CellPos = new Vector2Int(5, 5);
		EnterGame(monster);
		//
	}

	public void Update()
	{
		FlushJobs();

		//몬스러 리젠코드 (임시)
		if (_monsters.Values.Count == 0 && !bRespawning)
		{
			bRespawning = true;
			PushAfter(() =>
			{
				Monster monster = ObjectManager.Instance.AddObject<Monster>();
				monster.Init(1);
				monster.CellPos = new Vector2Int(5, 5);
				EnterGame(monster);
				bRespawning = false;
			}, 1000);
		}
	}

	public void EnterGame(GameObject gameObject)
	{
		if (gameObject == null)
			return;

		GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.ObjectId);

		if (type == GameObjectType.Player)
		{
			Player player = gameObject as Player;
			_players.Add(gameObject.ObjectId, player);
			player.Room = this;

			player.ReCalcAdditionalStat();

			Map.ApplyMove(player, new Vector2Int(player.PosInfo.PosX, player.PosInfo.PosY));
			//본인 클라이언트에도 접속되었음을 알림.
			{
				S_EnterGame enterPacket = new S_EnterGame();
				enterPacket.ObjectInfo = player.Info;
				player.Session.Send(enterPacket);

				S_Spawn spawnPacket = new S_Spawn();
				//기존에 접속해서 스폰되어있던 플레이어들을 알아야 클라이언트에서 똑같이 스폰할 수 있기때문에,
				//내가 들어왔을때 기존에 있었던 플레이어리스트를 나에게 전송.
				foreach (Player p in _players.Values)
				{
					if (gameObject != p)
						spawnPacket.ObjectInfos.Add(p.Info);
				}
				foreach (Monster m in _monsters.Values)
				{
					spawnPacket.ObjectInfos.Add(m.Info);
				}
				foreach (Projectile p in _projectiles.Values)
				{
					spawnPacket.ObjectInfos.Add(p.Info);
				}
				player.Session.Send(spawnPacket);
			}
		}
		else if (type == GameObjectType.Monster)
		{
			Monster monster = gameObject as Monster;
			_monsters.Add(gameObject.ObjectId, monster);
			monster.Room = this;

			Map.ApplyMove(monster, new Vector2Int(monster.PosInfo.PosX, monster.PosInfo.PosY));
			monster.Update();
		}
		else if (type == GameObjectType.Projectile)
		{
			Projectile projectile = gameObject as Projectile;
			_projectiles.Add(gameObject.ObjectId, projectile);
			projectile.Room = this;
			projectile.Update();
		}

		//타인한테도 들어왔다는 것을 알림.
		{
			S_Spawn spawnPacket = new S_Spawn();
			spawnPacket.ObjectInfos.Add(gameObject.Info);

			foreach (Player p in _players.Values)
			{
				if (p.ObjectId != gameObject.ObjectId)
				{
					p.Session.Send(spawnPacket);
				}
			}
		}
	}

	//GameRoom에서 Remove되면 전체 오브젝트매니저에서도 remove된다.
	public void LeaveGame(int objectId)
	{
		GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

		if (type == GameObjectType.Player)
		{
			Player player;
			if (_players.Remove(objectId, out player) == false)
				return;

			player.OnLeaveGame();
			Map.ApplyLeave(player);
			player.Room = null;

			//본인에게 정보전송.
			{
				S_LeaveGame leavePacket = new S_LeaveGame();
				player.Session.Send(leavePacket);
			}

		}
		else if (type == GameObjectType.Monster)
		{
			Monster monster = null;
			if (_monsters.Remove(objectId, out monster) == false)
				return;

			Map.ApplyLeave(monster);
			monster.Room = null;
		}
		else if (type == GameObjectType.Projectile)
		{
			Projectile projectile = null;
			if (_projectiles.Remove(objectId, out projectile) == false)
				return;

			Map.ApplyLeave(projectile);
			projectile.Room = null;
		}
		//타인에게 정보 전송.
		{
			S_Despawn despawnPacket = new S_Despawn();
			despawnPacket.ObjectIds.Add(objectId);
			foreach (Player p in _players.Values)
			{
				if (p.ObjectId != objectId)
				{
					p.Session.Send(despawnPacket);
				}
			}
		}

		ObjectManager.Instance.Remove(objectId);

	}

	public Player FindPlayer(Func<GameObject, bool> condition)
	{
		foreach (Player player in _players.Values)
		{
			if (condition.Invoke(player))
				return player;
		}
		return null;
	}

	public void Broadcast(IMessage packet)
	{
		foreach (Player p in _players.Values)
		{
			p.Session.Send(packet);
		}
	}

}

