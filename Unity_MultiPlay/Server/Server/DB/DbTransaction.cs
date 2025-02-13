using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DB;
using Server.GameContents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Server;

public partial class DbTransaction : JobSerializer // main함수의 while루프에서 flush하고 있음.
{
	public static DbTransaction Instance { get; } = new DbTransaction();

	public static void SaveDBPlayerStatus(Player player, GameRoom room)
	{
		//이 함수는 LeaveGame에서 불리고 있으므로, WorkerThread에서 처리중인 함수이다.
		if (player == null || room == null)
			return;

		//PlayerDb playerDb = db.Players.Find(PlayerDbId);//이코드는 DB Read를 해야하므로 느리다.
		PlayerDb playerDb = new PlayerDb();
		playerDb.PlayerDbId = player.PlayerDbId;//Id만 연결해주면, 수정될 row에 업데이트쿼리만 부를 수 있으면 되므로.
		playerDb.Hp = player.Hp;

		//여기까지는 호출쓰레드

		Instance.PushJob(() =>//Db쓰레드에서 플러쉬 될때 실행.
		{
			using (AppDbContext db = new AppDbContext())
			{
				db.Entry(playerDb).State = EntityState.Unchanged;
				db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
				bool success = db.SaveChangesEx();
				if (success)
				{
					room.PushJob(() =>//room에다 push한람다는 room 쓰레드에서 실행된다. 
					{
						Console.WriteLine($"PlayerStaus Saved (Hp : {playerDb.Hp})");//여기서는 tracked엔티티다.
					});
				}
			}
		});
		//job만 push하고 바로 리턴을 때린다.
		return;
	}

	public static async void SaveDBPlayerStatus_Async(Player player, GameRoom room)
	{
		if (player == null || room == null)
			return;

		//PlayerDb playerDb = db.Players.Find(PlayerDbId);//이코드는 DB Read를 해야하므로 느리다.
		PlayerDb playerDb = new PlayerDb();
		playerDb.PlayerDbId = player.PlayerDbId;//Id만 연결해주면, 수정될 row에 업데이트쿼리만 부를 수 있으면 되므로.
		playerDb.Hp = player.Hp;
		bool success = false;
		Task<bool> task = Task.Run(() =>
		{
			using (AppDbContext db = new AppDbContext())
			{
				db.Entry(playerDb).State = EntityState.Unchanged;
				db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
				return db.SaveChangesEx();
			}
		});

		success = await task;

		if (success)
		{
			room.PushJob(() =>
			{
				Console.WriteLine($"PlayerStaus Saved (Hp : {playerDb.Hp})");//여기서는 tracked엔티티다.
			});
		}
		return;
	}

	public static async void RewardPlayer(Player player, RewardData rewardData, GameRoom room)
	{
		if (player == null || rewardData == null || room == null)
			return;
		//TODO 살짝 문제가 있다.
		//1.DB에 먼저 저장요청-> 2. DB저장 완료후 서버메모리상에 적용-> 3.클라에 노티
		//이 방식은 동시에 같은 player가 이 함수를 호출했을때 slot번호가  
		int? Slot = player.Inventory.GetEmptySlotNumber();
		if (Slot == null)
			return;

		ItemDb itemDb = new ItemDb()
		{
			TemplateId = rewardData.ItemId,
			Count = rewardData.Count,
			SlotNumber = Slot.Value,
			OwnerDbId = player.PlayerDbId,
		};
		//1.DB에 먼저 저장요청
		bool success = false;
		Task<bool> task = Task.Run(() =>
		{
			using (AppDbContext db = new AppDbContext())
			{
				db.Items.Add(itemDb);
				return db.SaveChangesEx();
			}
		});

		success = await task;
		//2.DB저장 완료후 서버메모리상에 적용
		if (success)
		{
			room.PushJob(() =>
			{
				Console.WriteLine($"Item Added (ItemDbId : {itemDb.ItemDbId})");
				Item newItem = Item.CreateItemByItemDb(itemDb);
				player.Inventory.AddItem(newItem);
				//3.Notify to Client 
				{
					S_AddItems AddItemsPacket = new S_AddItems();
					ItemInfo itemInfo = new ItemInfo();
					itemInfo.MergeFrom(newItem.itemInfo);
					AddItemsPacket.ItemsInfos.Add(itemInfo);

					player.Session.Send(AddItemsPacket);
				}
			});
		}
	}

}
