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

        long _nextMoveTick = 0;
        long _PreTick = 0;
        long _DeltaTick = 0;
        public override void Update()
        {
            _DeltaTick = Environment.TickCount64 - _PreTick;
            //Console.WriteLine($"{Environment.TickCount64}, {_PreTick}, {_DeltaTick}");
            _PreTick = Environment.TickCount64;
            
            Vector2Int destPos = GetFrontCellPos();
            if (Owner == null || Room == null)
                return;
            if (skillData == null || skillData.projectileInfo ==null)
                return;

            if (CellPos != Owner.CellPos)
            {
                if (!Room.Map.CanGo(destPos) || !Room.Map.CanGo(CellPos))
                {
                    GameObject target = Room.Map.Find(destPos);
                    if (target != null)
                    {
                        int Damage = Owner.Stat.Attack + skillData.damage;
                        target.OnDamaged(this, Damage);
                    }
                    Room.PushJob(Room.LeaveGame, Id);
                    return;
                }
            }
            if (_nextMoveTick >= Environment.TickCount64)
            {
                return;
            }
                

            long tick = (long)(1000 / skillData.projectileInfo.speed);
            _nextMoveTick = Environment.TickCount64 + tick;

            //TODO 화살 콜리전 체크 더 자주 하도록 해야함.(삑사리)
            if (Room.Map.CanGo(destPos))
            {
                CellPos = destPos;
                S_Move movePacket = new S_Move();
                movePacket.ObjectId = Id;
                movePacket.PosInfo = PosInfo;
                Room.Broadcast(movePacket);
                Console.WriteLine("Move Arrow");
            }
            else
            {
                GameObject target = Room.Map.Find(destPos);
                if (target != null)
                {
                    int Damage = Owner.Stat.Attack + skillData.damage;
                    target.OnDamaged(this, Damage);
                    //TODO 피격판정
                }
                Room.PushJob(Room.LeaveGame, Id);
            }
        }
    }
}
