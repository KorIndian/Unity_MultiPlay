using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DB;
using Server.GameContents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server;

public partial class DbTransaction
{
	public static async void EquipItemNoti(Player player, Item item)
	{
		if(player == null || item == null) 
			return;

		ItemDb itemDb = new ItemDb()
		{
			ItemDbId = item.itemDbId,
			Equipped = item.Equipped
		};

		bool success = false;
		//만약 순서가 중요하다면 JobSerializer에 push하는 방식으로 만들어야겠지만,
		//순서가 딱히 중요하지 않으므로 Task로 만들어 쓰레드를 최대로 활용한다.
		Task<bool> task = Task.Run(() => {
			using (AppDbContext db = new AppDbContext())
			{
				db.Entry(itemDb).State = EntityState.Unchanged;
				db.Entry(itemDb).Property(nameof(itemDb.Equipped)).IsModified = true;

				return db.SaveChangesEx();
			}
		});
		//만약에 거래같은 중요한 데이터 트랜잭션의 경우 Task안에 할것들을 몰아넣고 마지막에 SaveChanges를 한번만 호출하며
		//결과가 true이면 client노티를 하는 방식으로 짜면 원자성을 보장할 수 있다.
		success = await task;
		if (success)
		{
			S_EquipItem equipOKItem = new S_EquipItem();
			equipOKItem.ItemDbId = item.itemDbId;
			equipOKItem.Equipped = item.Equipped;
			player.Session.Send(equipOKItem);
		}
		else
		{
			//TODO Handling
		}
	}
}
