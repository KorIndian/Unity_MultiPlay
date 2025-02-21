using DummyClient.Session;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

#pragma warning disable 8600

class PacketHandler
{
	//step 4
	public static void S_EnterGameHandler(PacketSession session, IMessage packet)
	{
		S_EnterGame enterGamePacket = packet as S_EnterGame;

	}

	public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
	{
		S_LeaveGame LeaveGamePacket = packet as S_LeaveGame;
	}

	public static void S_SpawnHandler(PacketSession session, IMessage packet)
	{
		S_Spawn SpawnPacket = packet as S_Spawn;
	}
	public static void S_DespawnHandler(PacketSession session, IMessage packet)
	{
		S_Despawn DespawnPacket = packet as S_Despawn;
	}
	public static void S_MoveHandler(PacketSession session, IMessage packet)
	{
		S_Move MovePacket = packet as S_Move;

	}

	public static void S_SkillHandler(PacketSession session, IMessage packet)
	{
		S_Skill SkillPacket = packet as S_Skill;
	}

	public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
	{
		S_ChangeHp hpPacket = packet as S_ChangeHp;

	}


	public static void S_DieHandler(PacketSession session, IMessage packet)
	{
		S_Die diePacket = packet as S_Die;
	}
	//step 1
	public static void S_ConnectedHandler(PacketSession session, IMessage packet)
	{
		C_LoginRequest LoginRequest = new C_LoginRequest();

		ServerSession serverSession = (ServerSession)session;
		LoginRequest.AccountName = $"DummyClient_{serverSession.DummyId}";
		serverSession.Send(LoginRequest);
	}

	//step2
	public static void S_LoginResultHandler(PacketSession session, IMessage packet)
	{
		S_LoginResult LoginResult = (S_LoginResult)packet;
		ServerSession serverSession = (ServerSession)session;
		//TODO 로비UI에서 캐릭터목록을 보여주고, 선택할 수 있도록
		if (LoginResult.Players == null || LoginResult.Players.Count == 0)
		{
			C_CreatePlayer createPlayerPkt = new C_CreatePlayer();
			createPlayerPkt.Name = $"Player_{serverSession.DummyId.ToString("0000")}";
			serverSession.Send(createPlayerPkt);
		}
		else
		{
			LobbyPlayerInfo playerInfo = LoginResult.Players[0];
			C_EnterGame enterGamePkt = new C_EnterGame();
			enterGamePkt.Name = playerInfo.Name;
			serverSession.Send(enterGamePkt);
		}
	}

	//step3
	public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
	{
		S_CreatePlayer createPlayerPacket = (S_CreatePlayer)packet;
		ServerSession serverSession = (ServerSession)session;
		if (createPlayerPacket.Player == null)
		{
			//더미 클라이언트에서는 재생성 시도를 생략한다.
		}
		else
		{
			C_EnterGame enterGamePkt = new C_EnterGame();
			enterGamePkt.Name = createPlayerPacket.Player.Name;
			serverSession.Send(enterGamePkt);
		}
	}

	public static void S_ItemInfolistHandler(PacketSession session, IMessage message)
	{
		S_ItemInfolist itemList = (S_ItemInfolist)message;

	}

	public static void S_AddItemsHandler(PacketSession session, IMessage message)
	{
		S_AddItems AddItems = (S_AddItems)message;

	}

	public static void S_EquipItemHandler(PacketSession session, IMessage message)
	{
		S_EquipItem equipItem = (S_EquipItem)message;

	}

	public static void S_ChangeStatHandler(PacketSession session, IMessage message)
	{
		S_ChangeStat ChangeStat = (S_ChangeStat)message;
	}

	public static void S_PingHandler(PacketSession session, IMessage message)
	{
	}
}