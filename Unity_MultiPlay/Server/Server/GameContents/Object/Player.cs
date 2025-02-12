using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Server.GameContents
{
    public class Player : GameObject
    {
        public int PlayerDbId { get; set; }
        public ClientSession Session { get; set; }
        public Inventory Inventory { get; private set; } = new Inventory();

		public int WeaponDamage { get; private set; } = 0;
		public int ArmorDefence { get; private set; } = 0;

		public override int TotalAttack { get => Stat.Attack + WeaponDamage; }
		public override int TotalDefence { get => ArmorDefence; }

		public Player()
        {
            ObjectType = GameObjectType.Player;
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {
            //TODO
            base.OnDamaged(attacker, damage);
			DbTransaction.SaveDBPlayerStatus(this, Room);
			
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
            DbTransaction.SaveDBPlayerStatus_Async(this, Room);
		}

		public void HandleEquipItem(C_EquipItem equipPacket)
        {
			Item item = Inventory.GetItem(equipPacket.ItemDbId);
			if (item == null)
				return;

			if (item.ItemType == ItemType.Consumable || item.ItemType == ItemType.None)
				return;

			//착용 요청이라면 같은 부위를 벗어준다.
			if (equipPacket.Equipped)
			{
				Item unEquipItem = null;

				if (item.ItemType == ItemType.Weapon)
				{
					//인벤토리에서 벗어야할 무기를 찾는다.
					unEquipItem = Inventory.FindItem(i => i.Equipped && i.ItemType == ItemType.Weapon);
				}
				else if (item.ItemType == ItemType.Armor)
				{
					ArmorType armorType = ((Armor)item).ArmorType;

					unEquipItem = Inventory.FindItem(
						i => i.Equipped && i.ItemType == ItemType.Armor
						&& ((Armor)i).ArmorType == armorType);
				}

				if (unEquipItem != null)
				{
					unEquipItem.Equipped = false;
					DbTransaction.EquipItemNoti(this, unEquipItem);
				}
			}

			item.Equipped = equipPacket.Equipped;
			DbTransaction.EquipItemNoti(this, item);

			ReCalcAdditionalStat();
		}

		public void ReCalcAdditionalStat()
		{
			WeaponDamage = 0;
			ArmorDefence = 0;

			foreach(Item item in Inventory.Items.Values)
			{
				if(item.Equipped == false)
					continue;

				switch (item.ItemType)
				{
					case ItemType.Weapon:
						WeaponDamage += ((Weapon)item).Damage;
						break;
					case ItemType.Armor:
						ArmorDefence += ((Armor)item).Defence;
						break;
				}
				
			}
		}
	}
}
