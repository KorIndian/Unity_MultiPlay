using System;
using System.Collections.Generic;
using System.Text;

namespace Server.GameContents
{
	class GameLogic : JobSerializer
	{
		public static GameLogic Instance { get; set; } = new GameLogic();

		Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();

		int GenRoomId = 1;

		public void Update()
		{
			FlushJobs();

			foreach (GameRoom room in _rooms.Values)
			{
				room.Update();
			}
		}

		public GameRoom CreateAndAddRoom(int mapId)
		{
			GameRoom gameRoom = new GameRoom();
			gameRoom.PushJob(gameRoom.Init, mapId, 10, 10);

			gameRoom.RoomId = GenRoomId;
			_rooms.Add(GenRoomId, gameRoom);
			GenRoomId++;

			return gameRoom;
		}

		public bool Remove(int roomId)
		{
			return _rooms.Remove(roomId);
		}

		public GameRoom Find(int roomId)
		{
			GameRoom room = null;
			if (_rooms.TryGetValue(roomId, out room))
				return room;

			return null;

		}
	}
}
