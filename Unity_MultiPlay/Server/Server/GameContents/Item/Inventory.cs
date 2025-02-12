using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.GameContents
{
    public class Inventory
    {
		public static int InventoryItemCount { get; private set; } = 54;
		Dictionary<int, Item> items = new Dictionary<int, Item>();

        public void Add(Item item)
        {
            items.Add(item.itemDbId, item);
		}

        public Item Get(int itemId)
        {
			items.TryGetValue(itemId, out var item);
            return item;
		}

        public Item Find(Func<Item, bool> condition)
        {
            return items.Values.ToList().Find(item => condition(item));
		}

        public int? GetEmptySlotNumber()
        {
            for(int slot =0; slot< InventoryItemCount; slot++)
            {
                Item FindItem = items.Values.FirstOrDefault(item => item.SlotNumber == slot);
                if (FindItem == null)
                    return slot;
            }
            return null;
        }
    }
}
