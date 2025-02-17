using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server.GameContents;

public class VisionBox
{
	public readonly Player Owner;

	public HashSet<GameObject> PreviousObjects { get; private set; } = new HashSet<GameObject>();

	object _lock = new object();

	public VisionBox(Player owner)
	{
		Owner = owner;
	}

	public void Update()
	{
		if (Owner == null || Owner.Room == null)
			return;

		//Console.WriteLine($"Player Zone ({Owner.CurrentZoneYIndex},{Owner.CurrentZoneXIndex})");

		HashSet<GameObject> CurrentObjects = GatherObjects();
		//기존에 없었는데 새로 생긴애들 spawn처리
		List<GameObject> Added = CurrentObjects.Except(PreviousObjects).ToList();
		if (Added.Count > 0)
		{
			S_Spawn spawnPacket = new S_Spawn();
			foreach (GameObject gameObject in Added)
			{
				ObjectInfo objectInfo = new ObjectInfo();
				objectInfo.MergeFrom(gameObject.Info);
				spawnPacket.ObjectInfos.Add(objectInfo);
				//바로 gameObject.Info를 넣어주지 않는 이유는 멀티쓰레드 환경에서 보내는 순간에도
				//info의 레퍼런스가 유효할지 보장이 없기 때문이다.
			}
			Owner.Session.Send(spawnPacket);
		}
		//기존엔 있었는데 사라진 애들 despawn처리
		List<GameObject> Removed = PreviousObjects.Except(CurrentObjects).ToList();
		if (Removed.Count > 0)
		{
			S_Despawn despawnPacket = new S_Despawn();
			foreach (GameObject gameObject in Removed)
			{
				despawnPacket.ObjectIds.Add(gameObject.ObjectId);
			}
			Owner.Session.Send(despawnPacket);
		}

		PreviousObjects = CurrentObjects;

		Owner.Room.PushAfter(Update, 200);
	}

	public HashSet<GameObject> GatherObjects()
	{
		if (Owner == null || Owner.Room == null)
			return null;
		HashSet<GameObject> objects = new HashSet<GameObject>();

		List<Zone> zones = Owner.Room.GetAdjecentZones(Owner.CellPos);

		foreach (Zone zone in zones)
		{
			//Console.WriteLine($"Adjecent Zone ({zone.IndexY},{zone.IndexX})");
			foreach (Player player in zone.Players)
			{
				if (player == null)
					continue;
				if (IsVisionBound(player.CellPos))
					objects.Add(player);
			}
			foreach (Monster monster in zone.Monsters)
			{
				if (monster == null)
					continue;
				if (IsVisionBound(monster.CellPos))
					objects.Add(monster);
			}
			foreach (Projectile projectile in zone.Projectiles)
			{
				if (projectile == null)
					continue;
				if (IsVisionBound(projectile.CellPos))
					objects.Add(projectile);
			}
		}

		return objects;
	}

	public bool IsVisionBound(Vector2Int cellPos)
	{
		int XDist = cellPos.x - Owner.CellPos.x;
		int YDist = cellPos.y - Owner.CellPos.y;

		if (Math.Abs(XDist) > GameRoom.VisionBounds
			|| Math.Abs(YDist) > GameRoom.VisionBounds)
			return false;

		return true;
	}

}
