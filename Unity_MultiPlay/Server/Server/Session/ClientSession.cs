using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;
using System.Net;
using Google.Protobuf.Protocol;
using Google.Protobuf;
using Server.GameContents;
using Server.Data;

namespace Server
{
	public partial class ClientSession : PacketSession
	{
		public PlayerServerState ServerState { get; private set; } = PlayerServerState.ServerStateLogin;

		public Player MyPlayer { get; set; }
		public int SessionId { get; set; }

		object _lock = new object();
		List<ArraySegment<byte>> _reserveQueue = new List<ArraySegment<byte>>();

		long _pingpongTick = 0;
		public void Ping()
		{
			if(_pingpongTick > 0)
			{
				long delta = System.Environment.TickCount64 - _pingpongTick;
				if(delta > 30000)//30초 이상응답이 없으면 디스커넥트 
				{
					Console.WriteLine($"Disconnected By PingPong Check SessionId: {SessionId}");
					Disconnect();
					return;
				}
				//TODO : 클라이언트가 디버깅중일때 튕길 수 있으므로 '개발단계'에서는 선택적으로 할 수 있도록해야한다.
			}

			S_Ping pingPacket = new S_Ping();
			Send(pingPacket);

			GameLogic.Instance.PushAfter(Ping, 5000);//5초 마다 한번씩 체크 
		}

		public void HandlePong()
		{
			_pingpongTick = System.Environment.TickCount64;
		}

		public void Send(IMessage packet)
        {
			string MessageName = packet.Descriptor.Name.Replace("_", string.Empty);//"SChat" 이런식으로 나옴.
			MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), MessageName);//MsgId중에서 MessageName과 이름이 같은 Enum값을 리턴해줌.
			ushort size = (ushort)packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 4];
            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
            //맨앞에 2바이트(ushort) size기입
            Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
            //그다음 2바이트는(ushort) 는 MsgId기입. 
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

			lock (_lock)
			{
				_reserveQueue.Add(sendBuffer);
			}
			//Send(new ArraySegment<byte>(sendBuffer));
        }

		public void FlushSend()
		{
			List<ArraySegment<byte>> sendList = null;
			lock (_lock)
			{
				if (_reserveQueue.Count == 0)
					return;

				sendList = _reserveQueue;
				_reserveQueue = new List<ArraySegment<byte>>();
			}

			Send(sendList);
		}

		public override void OnConnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnConnected : {endPoint}");
			{
				S_Connected connectedPacket = new S_Connected();
				Send(connectedPacket);
			}

			GameLogic.Instance.PushAfter(Ping, 5000);
		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
			GameLogic.Instance.PushJob(() =>
			{
				if (MyPlayer == null)
				{
					Console.WriteLine($"Warning : OnDisconnected() MyPlayer is null.");
					return;
				}
				GameRoom room = GameLogic.Instance.Find(1);
				room.PushJob(room.LeaveGame, MyPlayer.Info.ObjectId);
			});
			
			SessionManager.Instance.Remove(this);
			Console.WriteLine($"OnDisconnected : {endPoint}");
		}

		public override void OnSend(int numOfBytes)
		{
			//Console.WriteLine($"Transferred bytes: {numOfBytes}");
		}
	}
}
