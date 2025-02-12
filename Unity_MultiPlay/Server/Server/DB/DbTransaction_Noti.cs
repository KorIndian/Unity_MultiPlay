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
		Task<bool> task = Task.Run(() => {
			using (AppDbContext db = new AppDbContext())
			{
				db.Entry(itemDb).State = EntityState.Unchanged;
				db.Entry(itemDb).Property(nameof(itemDb.Equipped)).IsModified = true;

				return db.SaveChangesEx();
			}
		});

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
