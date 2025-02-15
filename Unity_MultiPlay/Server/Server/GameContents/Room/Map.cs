using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.GameContents
{
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

	public class Map
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

			Zone zone = gameObject.Room.GetZone(gameObject.CellPos);
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
				Zone CurrentZone = player.Room.GetZone(player.CellPos);
				Zone AfterZone = player.Room.GetZone(dest);
				if (CurrentZone != AfterZone)
				{
					CurrentZone.RemovePlayer(player);
					AfterZone.AddPlayer(player);
				}
			}
			else if (type == GameObjectType.Monster)
			{
				Monster monster = (Monster)gameObject;
				Zone CurrentZone = monster.Room.GetZone(monster.CellPos);
				Zone AfterZone = monster.Room.GetZone(dest);
				if (CurrentZone != AfterZone)
				{
					CurrentZone.RemoveMonster(monster);
					AfterZone.AddMonster(monster);
				}
			}
			else if (type == GameObjectType.Projectile)
			{
				Projectile projectile = (Projectile)gameObject;
				Zone CurrentZone = projectile.Room.GetZone(projectile.CellPos);
				Zone AfterZone = projectile.Room.GetZone(dest);
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

		#region A* PathFinding

		// U D L R (QuadTree)
		int[] _deltaY = new int[] { 1, -1, 0, 0 };
		int[] _deltaX = new int[] { 0, 0, -1, 1 };
		int[] _DirectionCost = new int[] { 10, 10, 10, 10 };

		public List<Vector2Int> FindPath(Vector2Int startCellPos, Vector2Int destCellPos, bool CheckObject = false, int maxDist = 10)
		{
			List<Pos> path = new List<Pos>();

			// 점수 매기기
			// F = G + H
			// F = 최종 점수 (작을 수록 좋음, 경로에 따라 달라짐)
			// G = 시작점에서 해당 좌표까지 이동하는데 드는 비용 (작을 수록 좋음, 경로에 따라 달라짐)
			// H = 목적지에서 얼마나 가까운지 (작을 수록 좋음, 고정)

			// (y, x) 이미 방문했는지 여부 (방문 = closed 상태)
			HashSet<Pos> CloseList = new HashSet<Pos>(); // CloseList

			// (y, x) 가는 길을 한 번이라도 발견했는지
			// 발견X => MaxValue
			// 발견O => F = G + H
			Dictionary<Pos, int> DynamicList = new Dictionary<Pos, int>();
			Dictionary<Pos, Pos> ParentChildLinks = new Dictionary<Pos, Pos>();

			// 오픈리스트에 있는 정보들 중에서, 가장 좋은 후보를 빠르게 뽑아오기 위한 도구
			PriorityQueue<PQNode> pq = new PriorityQueue<PQNode>();

			// CellPos -> ArrayPos
			Pos StartPos = Cell2Pos(startCellPos);
			Pos Dest = Cell2Pos(destCellPos);

			// 시작점 발견 (첫 스타트지점을 탐색 예약1번으로 넣고 while문을 돌린다.)
			//open[pos.Y, pos.X] = 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X));
			DynamicList.Add(StartPos, 10 * (Math.Abs(Dest.Y - StartPos.Y) + Math.Abs(Dest.X - StartPos.X)));
			pq.Push(new PQNode() { F = 10 * (Math.Abs(Dest.Y - StartPos.Y) + Math.Abs(Dest.X - StartPos.X)), G = 0, Y = StartPos.Y, X = StartPos.X });

			ParentChildLinks.Add(StartPos, StartPos);

			while (pq.Count > 0)
			{
				// 제일 좋은 후보를 찾는다(이미 F값에대한 우선순위큐로 구현되어 있기때문에 pop하는 것으로 끝난다.)
				PQNode PQnode = pq.Pop();
				Pos currentPos = new Pos(PQnode.Y, PQnode.X);
				// 동일한 좌표를 여러 경로로 찾아서, 더 빠른 경로로 인해서 이미 방문(closed)된 경우 스킵
				if (CloseList.Contains(currentPos))
					continue;

				CloseList.Add(currentPos);//방문했으면 Close에 넣는다.

				// 목적지 도착했으면 바로 종료
				if (PQnode.Y == Dest.Y && PQnode.X == Dest.X)
					break;

				// 상하좌우 등 이동할 수 있는 좌표인지 확인해서 탐색예약한다.
				for (int i = 0; i < _deltaY.Length; i++)//AStar는 기본적으로 Breast first search이다. 
				{
					Pos NextPos = new Pos(PQnode.Y + _deltaY[i], PQnode.X + _deltaX[i]);

					//탐색범위가 너무 멀면 스킵
					if (Math.Abs(currentPos.X - NextPos.X) + Math.Abs(currentPos.Y - NextPos.Y) > maxDist)
						continue;

					// 유효 범위를 벗어났으면 스킵, 벽으로 막혀서 갈 수 없으면 스킵(가지치기)
					if (NextPos.Y != Dest.Y || NextPos.X != Dest.X)
					{
						if (CanGo(Pos2Cell(NextPos), CheckObject) == false)
							continue;
					}

					// 이미 방문한 곳이면 스킵 (가지치기)
					if (CloseList.Contains(NextPos))
						continue;

					// 비용 계산
					int g = 0;// node.G + _cost[i];
					int h = 10 * ((Dest.Y - NextPos.Y) * (Dest.Y - NextPos.Y) + (Dest.X - NextPos.X) * (Dest.X - NextPos.X));

					int value = 0;
					if (DynamicList.TryGetValue(NextPos, out value) == false)
						value = int.MaxValue;

					// 다른 경로에서 더 빠른 길 이미 찾았으면
					if (value < g + h)//이쪽 방면은 유망하지않으므로 가지치기
						continue;

					if (DynamicList.TryAdd(NextPos, g + h) == false)//현재 NextPos가 오픈리스트에 있다면,
						DynamicList[NextPos] = g + h;//평가값을 덮어씌운다.

					// 다음 탐색 예약 진행
					pq.Push(new PQNode() { F = g + h, G = g, Y = NextPos.Y, X = NextPos.X });

					if (ParentChildLinks.TryAdd(NextPos, currentPos) == false)
						ParentChildLinks[NextPos] = currentPos;
				}

			}

			return MakePathsFromParents(ParentChildLinks, Dest);
		}

		List<Vector2Int> MakePathsFromParents(Dictionary<Pos, Pos> parentChildLinks, Pos dest)
		{
			List<Vector2Int> PathCells = new List<Vector2Int>();
			
			if(parentChildLinks.ContainsKey(dest) == false)
			{
				//목적지 까지 길이 없으면 찾은 셀중에서 가장 목적지와 가까운 곳을 목적지로 바꾼다.
				Pos bestPos = new Pos();
				int bestDist = int.MaxValue;
				
				foreach(Pos p in parentChildLinks.Keys)
				{
					int dist = Math.Abs(dest.Y - p.Y) + Math.Abs(dest.X - p.X);
					if(dist < bestDist)
					{
						bestPos = p;
						bestDist = dist; 
					}
				}

				dest = bestPos;
			}

			Pos pos = dest;
			while (parentChildLinks[pos] != pos)
			{
				PathCells.Add(Pos2Cell(pos));
				pos = parentChildLinks[pos];
			}
			PathCells.Add(Pos2Cell(pos));
			PathCells.Reverse();

			return PathCells;
		}

		Pos Cell2Pos(Vector2Int cell)
		{
			// CellPos -> ArrayPos
			return new Pos(MaxY - cell.y, cell.x - MinX);
		}

		Vector2Int Pos2Cell(Pos pos)
		{
			// ArrayPos -> CellPos
			return new Vector2Int(pos.X + MinX, MaxY - pos.Y);
		}

		#endregion
	}

}
