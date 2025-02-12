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

namespace Server
{
	public class DbTransction : JobSerializer // main함수의 while루프에서 flush하고 있음.
	{
		public static DbTransction Instance { get; } = new DbTransction();
	
		public static void SaveDBPlayerStatus(Player player, GameRoom room)
		{
			//이 함수는 LeaveGame에서 불리고 있으므로, WorkerThread에서 처리중인 함수이다.
			if (player == null || room == null)
				return;

			//PlayerDb playerDb = db.Players.Find(PlayerDbId);//이코드는 DB Read를 해야하므로 느리다.
			PlayerDb playerDb = new PlayerDb();
			playerDb.PlayerDbId = player.PlayerDbId;//Id만 연결해주면, 수정될 row에 업데이트쿼리만 부를 수 있으면 되므로.
			playerDb.Hp = player.Hp;

			//db접근은 시간이 오래걸리므로 job을 푸쉬에서 다른 쓰레드로 일감을 던진다 
			//아래의 람다는 메인쓰레드에서 처리될 예정이다. 
			Instance.PushJob(() => 
			{
				using (AppDbContext db = new AppDbContext())
				{
					db.Entry(playerDb).State = EntityState.Unchanged;
					db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
					bool success = db.SaveChangesEx();
					if (success)
					{
						//GameRoom은 timer쓰레드에서 update를 호출하고 update가 50ms마다 flush하고있다.
						room.PushJob(() =>
						{
                            Console.WriteLine($"PlayerStaus Saved (Hp : {playerDb.Hp})");//여기서는 tracked엔티티다.
                        });
					}
					else return;
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
			Task<bool> task = Task.Run(() => {
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

			bool success = false;
			Task<bool> task = Task.Run(() => {
				using (AppDbContext db = new AppDbContext())
				{
					db.Items.Add(itemDb);
					return db.SaveChangesEx();
				}
			});

			success = await task;
			if (success)
			{
				room.PushJob(() =>
				{
					Console.WriteLine($"Item Added (ItemDbId : {itemDb.ItemDbId})");//여기서는 tracked엔티티다.
					Item newItem = Item.CreateItemByItemDb(itemDb);
					player.Inventory.Add(newItem);
					//Notify to Client 
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
}
