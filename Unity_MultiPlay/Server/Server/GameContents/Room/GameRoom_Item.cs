using Google.Protobuf.Protocol;
using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.GameContents;

public partial class GameRoom
{
	public void HandleEquipItem(Player player, C_EquipItem equipPacket)
	{
		if (player == null)
			return;

		player.HandleEquipItem(equipPacket);
	}
}
