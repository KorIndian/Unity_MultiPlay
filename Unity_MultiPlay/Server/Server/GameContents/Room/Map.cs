using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.GameContents;

public struct Pos
{
	public Pos(int y, int x) { Y = y; X = x; }
	public int Y;
	public int X;

	public static bool operator ==(Pos left, Pos right)
	{
		return left.Y == right.Y && left.X == right.X;
	}

	public static bool operator !=(Pos left, Pos right)
	{
		return !(left == right);
	}

	public override bool Equals(object obj)
	{
		return (Pos)obj == this;
	}

	public override int GetHashCode()
	{
		long Val = (Y << 32) | X;
		return Val.GetHashCode();
	}
}

public struct PQNode : IComparable<PQNode>
{
	public int F;
	public int G;
	public int Y;
	public int X;

	public int CompareTo(PQNode other)
	{
		if (F == other.F)
			return 0;
		return F < other.F ? 1 : -1;
	}
}

public struct Vector2Int
{
	public int x;
	public int y;

	public Vector2Int(int x, int y) { this.x = x; this.y = y; }

	public static Vector2Int up { get { return new Vector2Int(0, 1); } }
	public static Vector2Int down { get { return new Vector2Int(0, -1); } }
	public static Vector2Int left { get { return new Vector2Int(-1, 0); } }
	public static Vector2Int right { get { return new Vector2Int(1, 0); } }

	public static Vector2Int operator +(Vector2Int a, Vector2Int b)
	{
		return new Vector2Int(a.x + b.x, a.y + b.y);
	}

	public static Vector2Int operator -(Vector2Int a, Vector2Int b)
	{
		return new Vector2Int(a.x - b.x, a.y - b.y);
	}

	public static bool operator ==(Vector2Int a, Vector2Int b)
	{
		return a.x == b.x && a.y == b.y;
	}

	public static bool operator !=(Vector2Int a, Vector2Int b)
	{
		return !(a == b);
	}

	public static int GetCellDist(Vector2Int a, Vector2Int b)
	{
		return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
	}

	public override bool Equals(object obj)
	{
		return obj is Vector2Int @vec &&
			   x == @vec.x &&
			   y == @vec.y;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(x, y);
	}

	public int CellDistFromZero
	{
		get { return Math.Abs(x) + Math.Abs(y); }
	}

	public float Magnitude
	{
		get { return (float)Math.Sqrt(SqrMagnitude); }
	}

	public int SqrMagnitude
	{
		get { return (x * x + y * y); }
	}

}

public partial class Map
{
	public int MinX { get; set; }
	public int MaxX { get; set; }
	public int MinY { get; set; }
	public int MaxY { get; set; }

	public int SizeX { get { return MaxX - MinX + 1; } }
	public int SizeY { get { return MaxY - MinY + 1; } }

	bool[,] _collision;
	GameObject[,] _objects;

	public bool CanGo(Vector2Int cellPos, bool checkObjects = true)
	{
		if (cellPos.x < MinX || cellPos.x > MaxX)
			return false;
		if (cellPos.y < MinY || cellPos.y > MaxY)
			return false;

		int x = cellPos.x - MinX;
		int y = MaxY - cellPos.y;
		return !_collision[y, x] && (!checkObjects || _objects[y, x] == null);
	}

	public GameObject Find(Vector2Int cellPos)
	{
		if (cellPos.x < MinX || cellPos.x > MaxX)
			return null;
		if (cellPos.y < MinY || cellPos.y > MaxY)
			return null;

		//Vector2Int 는 원점이 좌하단 좌표계를 쓰고
		//Map의 cell좌표계는 원점이 좌측 상단에 있기때문에 좌표계를 변환해주는 것이다.
		int x = cellPos.x - MinX;
		int y = MaxY - cellPos.y;
		return _objects[y, x];
	}

	public bool ApplyLeave(GameObject gameObject)
	{
		if (gameObject.Room == null)
			return false;
		if (gameObject.Room.Map != this)
			return false;

		PositionInfo posInfo = gameObject.PosInfo;
		if (posInfo.PosX < MinX || posInfo.PosX > MaxX)
			return false;
		if (posInfo.PosY < MinY || posInfo.PosY > MaxY)
			return false;

		Zone zone = gameObject.Room.GetZoneByCellPos(gameObject.CellPos);
		zone.RemoveObject(gameObject);

		{
			int x = posInfo.PosX - MinX;
			int y = MaxY - posInfo.PosY;
			if (_objects[y, x] == gameObject)
				_objects[y, x] = null;
		}

		return true;
	}

	public bool ApplyMove(GameObject gameObject, Vector2Int dest, bool checkObjects = true, bool applyCollision =true)
	{
		if (gameObject.Room == null)
			return false;
		if (gameObject.Room.Map != this)
			return false;

		PositionInfo posInfo = gameObject.PosInfo;
		if (CanGo(dest, checkObjects) == false)
			return false;

		if(applyCollision)
		{
			{
				int x = posInfo.PosX - MinX;
				int y = MaxY - posInfo.PosY;
				if (_objects[y, x] == gameObject)
					_objects[y, x] = null;
			}

			{
				int x = dest.x - MinX;
				int y = MaxY - dest.y;
				_objects[y, x] = gameObject;
			}
		}

		GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.ObjectId);
		if (type == GameObjectType.Player)
		{
			Player player = (Player)gameObject;
			Zone CurrentZone = player.Room.GetZoneByCellPos(player.CellPos);
			Zone AfterZone = player.Room.GetZoneByCellPos(dest);
			if (CurrentZone != AfterZone)
			{
				CurrentZone.RemovePlayer(player);
				AfterZone.AddPlayer(player);
			}
		}
		else if (type == GameObjectType.Monster)
		{
			Monster monster = (Monster)gameObject;
			Zone CurrentZone = monster.Room.GetZoneByCellPos(monster.CellPos);
			Zone AfterZone = monster.Room.GetZoneByCellPos(dest);
			if (CurrentZone != AfterZone)
			{
				CurrentZone.RemoveMonster(monster);
				AfterZone.AddMonster(monster);
			}
		}
		else if (type == GameObjectType.Projectile)
		{
			Projectile projectile = (Projectile)gameObject;
			Zone CurrentZone = projectile.Room.GetZoneByCellPos(projectile.CellPos);
			Zone AfterZone = projectile.Room.GetZoneByCellPos(dest);
			if (CurrentZone != AfterZone)
			{
				CurrentZone.RemoveProjectile(projectile);
				AfterZone.AddProjectile(projectile);
			}
		}

		// 실제 좌표 이동
		posInfo.PosX = dest.x;
		posInfo.PosY = dest.y;
		return true;
	}

	public void LoadMap(int mapId, string pathPrefix = "../../../../../Common/MapData")
	{
		string mapName = "Map_" + mapId.ToString("000");

		// Collision 관련 파일
		string text = File.ReadAllText($"{pathPrefix}/{mapName}.txt");
		StringReader reader = new StringReader(text);

		MinX = int.Parse(reader.ReadLine());
		MaxX = int.Parse(reader.ReadLine());
		MinY = int.Parse(reader.ReadLine());
		MaxY = int.Parse(reader.ReadLine());
		
		int xCount = MaxX - MinX + 1;
		int yCount = MaxY - MinY + 1;
		_collision = new bool[yCount, xCount];
		_objects = new GameObject[yCount, xCount];

		for (int y = 0; y < yCount; y++)
		{
			string line = reader.ReadLine();
			for (int x = 0; x < xCount; x++)
			{
				_collision[y, x] = (line[x] == '1' ? true : false);
			}
		}
	}

	protected Pos Cell2Pos(Vector2Int cell)
	{
		// CellPos -> ArrayPos
		return new Pos(MaxY - cell.y, cell.x - MinX);
	}

	protected Vector2Int Pos2Cell(Pos pos)
	{
		// ArrayPos -> CellPos
		return new Vector2Int(pos.X + MinX, MaxY - pos.Y);
	}

}
