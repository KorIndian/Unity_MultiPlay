using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.GameContents
{
    public class ObjectManager
    {
        public static ObjectManager Instance { get; set; } = new ObjectManager();

        object _lock = new object();
        Dictionary<int, GameObject> _gameObjects = new Dictionary<int, GameObject>();
        Dictionary<int, Player> _players = new Dictionary<int, Player>();

        //Id 32비트 [UNUSED(1)][TPYE(7)][ID(24)] 
        int _counter = 1;

        //↓↓ T는 GameObject하위클래스여야하며, new로 할당 가능해햐한다.
        public T AddObject<T>() where T : GameObject, new()
        {
            T gameObject = new T();
            lock (_lock)
            {
                gameObject.Id = GenerateId(gameObject.ObjectType);

                _gameObjects.Add(gameObject.Id, gameObject);

                if (gameObject.ObjectType == GameObjectType.Player)
                {
                    _players.Add(gameObject.Id, gameObject as Player);
                }
            }
            return gameObject;
        }

        private int GenerateId(GameObjectType type)
        {
            lock (_lock)
            {
                return ((int)type << 24 | _counter++);
            }
        }

        public static GameObjectType GetObjectTypeById(int id)
        {
            int type = id >> 24 & 0x7F;
            return (GameObjectType)type;
        }

        public bool Remove(int ObjectId)
        {
            lock (_lock)
            {
                if (GetObjectTypeById(ObjectId) == GameObjectType.Player)
                {
                    _players.Remove(ObjectId);
                }

                return _gameObjects.Remove(ObjectId);
            }
        }

        public T Find<T>(int ObjectId) where T : GameObject, new()
        {
            lock (_lock)
            {
                GameObject go = null;
                T Obj = null;
                if (_gameObjects.TryGetValue(ObjectId, out go))
                {
                    Obj = go as T;
                    return Obj;
                }
                return null;
            }
        }

    }
}
