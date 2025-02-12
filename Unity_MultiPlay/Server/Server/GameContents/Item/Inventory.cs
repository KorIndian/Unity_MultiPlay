using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.GameContents
{
    public class Inventory
    {
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
    }
}
