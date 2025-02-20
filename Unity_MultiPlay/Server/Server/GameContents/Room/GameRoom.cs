using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Server.GameContents;

public partial class GameRoom : JobSerializer
{
	public int RoomId { get; set; }
	Dictionary<int, Player> _players = new Dictionary<int, Player>();
	Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
	Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();

	public Zone[,] Zones { get; private set; }
	public int ZoneWidth { get; private set; }
	public int ZoneHeight { get; private set; }

	public Map Map { get; private set; } = new Map();

	public int MonsterMaxCount { get; private set; } = 500;

	public const int VisionBounds = 7;

	bool bRespawning = false;

	public void Init(int mapId, int zoneWidth, int zoneHeight)
	{
		Map.LoadMap(mapId, "../../../../../Common/MapData");

		ZoneWidth = zoneWidth;
		ZoneHeight = zoneHeight;

		int countY = (Map.SizeY + zoneHeight - 1) / zoneHeight;
		int countX = (Map.SizeX + zoneWidth - 1) / zoneWidth;
		Zones = new Zone[countY, countX];

		for (int y = 0; y < countY; ++y)
		{
			for (int x = 0; x < countX; ++x)
			{
				Zones[y, x] = new Zone(y, x);
			}
		}

		for(int i = 0; i< MonsterMaxCount; i++)
		{
			Monster monster = ObjectManager.Instance.AddObject<Monster>();
			monster.InitByTemplatedId(1);
			EnterGame(monster, true);
		}
		LogMonsterCount();
	}

	public void LogMonsterCount()
	{
		Console.WriteLine($"Monster Count : {_monsters.Count}");
		PushAfter(LogMonsterCount, 10000);
	}

	public void Update()
	{
		FlushJobs();

		//if (MonsterMaxCount > _monsters.Values.Count)
		//{
		//	Monster monster = ObjectManager.Instance.AddObject<Monster>();
		//	monster.InitByTemplatedId(1);
		//	EnterGame(monster, true);
		//	return;
		//}
	}

	public Zone GetZoneByIndex(int indexY, int indexX)
	{
		if (indexX < 0 || indexX >= Zones.GetLength(1))
			return null;
		if (indexY < 0 || indexY >= Zones.GetLength(0))
			return null;

		return Zones[indexY, indexX];
	}

	public Zone GetZoneByCellPos(Vector2Int cellPos)
	{
		//Vector2Int 는 원점이 좌하단 좌표계를 쓰고
		//Map의 cell좌표계는 원점이 좌측 상단에 있기때문에 좌표계를 변환해주는 것이다.
		int Xindex = (cellPos.x - Map.MinX) / ZoneWidth;
		int Yindex = (Map.MaxY - cellPos.y) / ZoneHeight;

		return GetZoneByIndex(Yindex, Xindex);
	}

	Random rand = new Random();
	public void EnterGame(GameObject gameObject, bool bRandomPos = false)
	{
		if (gameObject == null)
			return;

		if(bRandomPos)
		{
			Vector2Int randomPos = new();
			while (true)
			{
				randomPos.x = rand.Next(Map.MinX, Map.MaxX + 1);
				randomPos.y = rand.Next(Map.MinY, Map.MaxY + 1);
				if (Map.Find(randomPos) == null && Map.CanGo(randomPos))
				{
					gameObject.CellPos = randomPos;
					break; 
				}
			}
		}

		GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.ObjectId);

		if (type == GameObjectType.Player)
		{
			Player player = gameObject as Player;
			_players.Add(gameObject.ObjectId, player);
			player.Room = this;

			player.ReCalcAdditionalStat();

			Map.ApplyMove(player, new Vector2Int(player.PosInfo.PosX, player.PosInfo.PosY));
			GetZoneByCellPos(player.CellPos).AddPlayer(player);

			//본인 클라이언트에도 접속되었음을 알림.
			{
				S_EnterGame enterPacket = new S_EnterGame();
				enterPacket.ObjectInfo = player.Info;
				player.Session.Send(enterPacket);

				//S_Spawn spawnPacket = new S_Spawn();
				////기존에 접속해서 스폰되어있던 플레이어들을 알아야 클라이언트에서 똑같이 스폰할 수 있기때문에,
				////내가 들어왔을때 기존에 있었던 플레이어리스트를 나에게 전송.
				//foreach (Player p in _players.Values)
				//{
				//	if (gameObject != p)
				//		spawnPacket.ObjectInfos.Add(p.Info);
				//}
				//foreach (Monster m in _monsters.Values)
				//{
				//	spawnPacket.ObjectInfos.Add(m.Info);
				//}
				//foreach (Projectile p in _projectiles.Values)
				//{
				//	spawnPacket.ObjectInfos.Add(p.Info);
				//}
				//player.Session.Send(spawnPacket);

				player.VisibleBox.Update();
			}
		}
		else if (type == GameObjectType.Monster)
		{
			Monster monster = gameObject as Monster;
			_monsters.Add(gameObject.ObjectId, monster);
			monster.Room = this;

			Map.ApplyMove(monster, new Vector2Int(monster.PosInfo.PosX, monster.PosInfo.PosY));
			GetZoneByCellPos(monster.CellPos).AddMonster(monster);
			monster.Update();
		}
		else if (type == GameObjectType.Projectile)
		{
			Projectile projectile = gameObject as Projectile;
			_projectiles.Add(gameObject.ObjectId, projectile);

			GetZoneByCellPos(projectile.CellPos).AddProjectile(projectile);

			projectile.Room = this;
			projectile.Update();
		}

		//타인한테도 들어왔다는 것을 알림.
		{
			S_Spawn spawnPacket = new S_Spawn();
			spawnPacket.ObjectInfos.Add(gameObject.Info);

			BroadcastVisionBound(gameObject.CellPos, spawnPacket);
		}
	}

	//GameRoom에서 Remove되면 전체 오브젝트매니저에서도 remove된다.
	public void LeaveGame(int objectId)
	{
		GameObjectType type = ObjectManager.GetObjectTypeById(objectId);
		Vector2Int cellPos = new();
		if (type == GameObjectType.Player)
		{
			Player player;
			if (_players.Remove(objectId, out player) == false)
				return;
			cellPos = player.CellPos;
			
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
			cellPos = monster.CellPos;
			
			Map.ApplyLeave(monster);
			monster.Room = null;
		}
		else if (type == GameObjectType.Projectile)
		{
			Projectile projectile = null;
			if (_projectiles.Remove(objectId, out projectile) == false)
				return;
			cellPos = projectile.CellPos;
			
			Map.ApplyLeave(projectile);
			projectile.Room = null;
		}
		//타인에게 정보 전송.
		{
			S_Despawn despawnPacket = new S_Despawn();
			despawnPacket.ObjectIds.Add(objectId);
			BroadcastVisionBound(cellPos, despawnPacket);
		}
		ObjectManager.Instance.Remove(objectId);
	}

	public void BroadcastVisionBound(Vector2Int cellPos, IMessage packet)
	{
		List<Zone> zones = GetAdjacentZones(cellPos);

		foreach (Player player in zones.SelectMany(z => z.Players))
		{
			int XDist = player.CellPos.x - cellPos.x;
			int YDist = player.CellPos.y - cellPos.y;

			if (Math.Abs(XDist) > GameRoom.VisionBounds
				|| Math.Abs(YDist) > GameRoom.VisionBounds)
				continue;

			player.Session.Send(packet);
		}
	}

	public void BroadcastAdjecentZones(Vector2Int cellPos, IMessage packet)
	{
		List<Zone> zones = GetAdjacentZones(cellPos);
		foreach (Player player in zones.SelectMany(z => z.Players))
		{
			player.Session.Send(packet);
		}
	}

	public void BroadcastToAll(IMessage packet)
	{
		foreach (Player p in _players.Values)
		{
			p.Session.Send(packet);
		}
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
	
	public Player FindClosestPlayer(Vector2Int pos, int range)
	{
		List<Player> players = GetAdjacentPlayers(pos, range);

		//가까운 순으로 정렬
		players.Sort((left, right) =>
		{
			int leftDist = (left.CellPos - pos).CellDistFromZero;
			int rightDist = (right.CellPos - pos).CellDistFromZero;
			return leftDist - rightDist;
		});

		foreach (Player player in players)
		{
			List<Vector2Int> path = Map.FindPath(pos, player.CellPos, CheckObject: true);
			if (path.Count < 2 || path.Count > range)
				continue;

			//발견하면 바로 리턴한다.
			return player;
		}

		return null;
	}

	public List<Zone> GetAdjacentZones(Vector2Int cellPos, int bounds = VisionBounds)
	{
		HashSet<Zone> zones = new HashSet<Zone>();

		int maxY = cellPos.y + bounds;
		int minY = cellPos.y - bounds;
		int maxX = cellPos.x + bounds;
		int minX = cellPos.x - bounds;

		// 좌측 상단
		Vector2Int leftTop = new Vector2Int(minX, maxY);
		int minIndexY = (Map.MaxY - leftTop.y) / ZoneHeight;
		int minIndexX = (leftTop.x - Map.MinX) / ZoneWidth;

		// 우측 하단
		Vector2Int rightBot = new Vector2Int(maxX, minY);
		int maxIndexY = (Map.MaxY - rightBot.y) / ZoneHeight;
		int maxIndexX = (rightBot.x - Map.MinX) / ZoneWidth;

		for (int Xidx = minIndexX; Xidx <= maxIndexX; Xidx++)
		{
			for (int YIdx = minIndexY; YIdx <= maxIndexY; YIdx++)
			{
				Zone zone = GetZoneByIndex(YIdx, Xidx);
				if (zone == null)
					continue;

				zones.Add(zone);
			}
		}

		return zones.ToList();
	}

	public List<Player> GetAdjacentPlayers(Vector2Int pos, int range)
	{
		List<Zone> zones = GetAdjacentZones(pos);
		return zones.SelectMany(z=>z.Players).ToList();
	}

}

