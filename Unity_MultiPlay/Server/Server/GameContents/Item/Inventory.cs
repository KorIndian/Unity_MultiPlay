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
		public Dictionary<int, Item> Items = new Dictionary<int, Item>();

        public void AddItem(Item item)
        {
            Items.Add(item.itemDbId, item);
		}

        public Item GetItem(int itemId)
        {
			Items.TryGetValue(itemId, out var item);
            return item;
		}

        public Item FindItem(Func<Item, bool> condition)
        {
            return Items.Values.ToList().Find(item => condition(item));
		}

        public int? GetEmptySlotNumber()
        {
            for(int slot =0; slot< InventoryItemCount; slot++)
            {
                Item FindItem = Items.Values.FirstOrDefault(item => item.SlotNumber == slot);
                if (FindItem == null)
                    return slot;
            }
            return null;
        }
    }
}
