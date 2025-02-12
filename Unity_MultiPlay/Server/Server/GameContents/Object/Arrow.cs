using Google.Protobuf.Protocol;
using Server.GameContents;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.GameContents
{
    public class Arrow : Projectile
    {
        public GameObject Owner { get; set; }

        public override void Update()
        {
			if (Owner == null || Room == null)
				return;

			if (skillData == null || skillData.projectileInfo == null)
				return;

			int tick = (int)(1000 / skillData.projectileInfo.speed);//e.g (1000/5)ms -> 0.2초에 한번씩 업데이트를 예약한다.
			Room.PushAfter(Update, tick);
			Vector2Int destPos = GetFrontCellPos();
            
            if (CellPos != Owner.CellPos)
            {
                if (!Room.Map.CanGo(destPos) || !Room.Map.CanGo(CellPos))
                {
                    GameObject target = Room.Map.Find(destPos);
                    if (target != null)
                    {
                        int Damage = Owner.TotalAttack + skillData.damage;
                        target.OnDamaged(this, Damage);
                    }
                    Room.PushJob(Room.LeaveGame, ObjectId);
                    return;
                }
            }
            
            //TODO 화살 콜리전 체크 더 자주 하도록 해야함.(삑사리)
            if (Room.Map.CanGo(destPos))
            {
                CellPos = destPos;
                S_Move movePacket = new S_Move();
                movePacket.ObjectId = ObjectId;
                movePacket.PosInfo = PosInfo;
                Room.Broadcast(movePacket);
                Console.WriteLine("Move Arrow");
            }
            else
            {
                GameObject target = Room.Map.Find(destPos);
                if (target != null)
                {
                    int Damage = Owner.TotalAttack + skillData.damage;
                    target.OnDamaged(this, Damage);
                    //TODO 피격판정
                }
                Room.PushJob(Room.LeaveGame, ObjectId);
            }
			
		}

		public override GameObject GetOwner()
		{
			return Owner;
		}
	}
}
