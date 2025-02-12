using System;
using System.Collections.Generic;
using System.Text;

namespace Server.GameContents
{
    class RoomManager
    {
        public static RoomManager Instance { get; set; } = new RoomManager();
        object _lock = new object();
        Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
        int GenRoomId = 1;

        public GameRoom CreateAndAddRoom(int mapId)
        {
            GameRoom gameRoom = new GameRoom();
            gameRoom.PushJob(gameRoom.Init, mapId);

            lock(_lock)
            {
                gameRoom.RoomId = GenRoomId;
                _rooms.Add(GenRoomId, gameRoom);
                GenRoomId++;
            }
            return gameRoom;
        }

        public bool Remove(int roomId)
        {
            lock(_lock)
            {
                return _rooms.Remove(roomId);
            }
        }

        public GameRoom Find(int roomId)
        {
            lock(_lock)
            {
                GameRoom room = null;
                if (_rooms.TryGetValue(roomId, out room))
                    return room;

                return null;
            }
        }
    }
}
