using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.GameContents;

public partial class Map
{
	#region A* PathFinding
	// U D L R (QuadTree)
	int[] _deltaY = new int[] { 1, -1, 0, 0 };
	int[] _deltaX = new int[] { 0, 0, -1, 1 };
	int[] _DirectionCost = new int[] { 10, 10, 10, 10 };

	public List<Vector2Int> FindPath(Vector2Int startCellPos, Vector2Int destCellPos, bool CheckObject = false, int maxDist = 8)
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
			if (currentPos.Y == Dest.Y && currentPos.X == Dest.X)
				break;

			// 상하좌우 등 이동할 수 있는 좌표인지 확인해서 탐색예약한다.
			for (int i = 0; i < _deltaY.Length; i++)//AStar는 기본적으로 Breast first search이다. 
			{
				Pos NextPos = new Pos(currentPos.Y + _deltaY[i], currentPos.X + _deltaX[i]);

				//탐색범위가 너무 멀면 스킵
				if (Math.Abs(StartPos.X - NextPos.X) + Math.Abs(StartPos.Y - NextPos.Y) > maxDist)
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
					value = Int32.MaxValue;

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

		if (parentChildLinks.ContainsKey(dest) == false)
		{
			//목적지 까지 길이 없으면 찾은 셀중에서 가장 목적지와 가까운 곳을 목적지로 바꾼다.
			Pos bestPos = new Pos();
			int bestDist = int.MaxValue;

			foreach (Pos p in parentChildLinks.Keys)
			{
				int dist = Math.Abs(dest.Y - p.Y) + Math.Abs(dest.X - p.X);
				if (dist < bestDist)
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

	#endregion
}


