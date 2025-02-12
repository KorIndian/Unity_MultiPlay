using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.GameContents
{
    public class Player : GameObject
    {
        public int PlayerDbId { get; set; }
        public ClientSession Session { get; set; }
        public Inventory Inventory { get; private set; } = new Inventory();

        public Player()
        {
            ObjectType = GameObjectType.Player;
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {
            //TODO
            base.OnDamaged(attacker, damage);
			DbTransction.SaveDBPlayerStatus(this, Room);
			Console.WriteLine($"Damage : {damage}");
        }

        public override void OnDead(GameObject attacker)
        {
            GameRoom room = Room;
            base.OnDead(attacker);

            Stat.Hp = Stat.MaxHp;
            PosInfo.State = CreatureState.Idle;
            PosInfo.MoveDir = MoveDir.Down;
            PosInfo.PosX = 0;
            PosInfo.PosY = 0;

            room.EnterGame(this);
        }

        public void OnLeaveGame()//OnDisConnected시에 호출된다.
        {
            //DbTransction.SaveDBPlayerStatus(this, Room);
            DbTransction.SaveDBPlayerStatus_Async(this, Room);
		}
    }
}
