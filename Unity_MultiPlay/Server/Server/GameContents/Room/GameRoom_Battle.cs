using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.GameContents;

public partial class GameRoom
{
	public void HandleMove(Player player, C_Move movePacket)
	{
		//TODO : 이동 할 수 있는 위치인지 판정.
		if (player == null)
			return;
		//▼아래 로직은 공유데이터를 많이 접근하는 코드이므로 lock이 필요하다.
		//따라서 락을 소유하는 room에서 처리하는 것이 안전하다.

		PositionInfo movePosInfo = movePacket.PosInfo;

		ObjectInfo info = player.Info;

		//현재좌표와 다른 좌표로 가고싶다는 이동 패킷이 왔을때 갈 수 있는 위치인지 확인
		if (movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY)
		{
			if (Map.CanGo(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == false)
				return;
		}
		info.PosInfo.State = movePacket.PosInfo.State;
		info.PosInfo.MoveDir = movePacket.PosInfo.MoveDir;
		Map.ApplyMove(player, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY));

		S_Move resMovePacket = new S_Move();
		resMovePacket.ObjectId = player.Info.ObjectId;
		resMovePacket.PosInfo = movePacket.PosInfo;

		BroadcastVisionBound(player.CellPos, resMovePacket);
	}

	public void HandleSkill(Player player, C_Skill skillPacket)
	{
		if (player == null)
			return;

		ObjectInfo info = player.Info;
		if (info.PosInfo.State != CreatureState.Idle)
			return;

		//TODO: 스킬 사용 가능 여부 체크.
		info.PosInfo.State = CreatureState.Skill;
		S_Skill ServerSkillPacket = new S_Skill() { Info = new SkillInfo() };

		ServerSkillPacket.ObjectId = info.ObjectId;
		ServerSkillPacket.Info.SkillId = skillPacket.Info.SkillId;
		BroadcastVisionBound(player.CellPos, ServerSkillPacket);

		SkillData skillData = null;
		if (DataManager.SkillDict.TryGetValue(skillPacket.Info.SkillId, out skillData) == false)
			return;

		switch (skillData.skillType)
		{
			case SkillType.SkillAuto:
				{
					//TODO 대미지 판정.
					Vector2Int skillPos = player.GetFrontCellPos(info.PosInfo.MoveDir);
					GameObject target = Map.Find(skillPos);
					if (target != null)
					{
						Console.WriteLine("Hit Player !");
					}
				}
				break;
			case SkillType.SkillProjectile:
				{
					//TODO : Arrow
					Arrow arrow = ObjectManager.Instance.AddObject<Arrow>();
					if (arrow == null)
						return;
					arrow.Owner = player;
					arrow.skillData = skillData;
					arrow.PosInfo.State = CreatureState.Moving;
					arrow.PosInfo.MoveDir = player.PosInfo.MoveDir;
					Vector2Int skillPos = player.GetFrontCellPos(info.PosInfo.MoveDir);
					arrow.PosInfo.PosX = player.PosInfo.PosX;
					arrow.PosInfo.PosY = player.PosInfo.PosY;
					arrow.Speed = skillData.projectileInfo.speed;
					PushJob(EnterGame, arrow, false);
				}
				break;
		}

	}
}
