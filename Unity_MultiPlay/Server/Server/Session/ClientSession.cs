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
		public Player MyPlayer { get; set; }
		public int SessionId { get; set; }

		public PlayerServerState ServerState { get; private set; } = PlayerServerState.ServerStateLogin; 

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

            Send(new ArraySegment<byte>(sendBuffer));
        }

		public override void OnConnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnConnected : {endPoint}");
			{
				S_Connected connectedPacket = new S_Connected();
				Send(connectedPacket);
			}
        }

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
			GameRoom room = RoomManager.Instance.Find(1);
			room.PushJob(room.LeaveGame, MyPlayer.Info.ObjectId);
			
			SessionManager.Instance.Remove(this);
			Console.WriteLine($"OnDisconnected : {endPoint}");
		}

		public override void OnSend(int numOfBytes)
		{
			//Console.WriteLine($"Transferred bytes: {numOfBytes}");
		}
	}
}
