using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Server.Data;
using Server.DB;
using Server.GameContents;
using ServerCore;

namespace Server
{
	class Program
	{
		static Listener _listener = new Listener();
		static List<System.Timers.Timer> _timers = new List<System.Timers.Timer>();

		static void TickRoom(GameRoom room, int tick = 100)
        {
			var timer = new System.Timers.Timer();
			timer.Interval = tick;
			timer.Elapsed += ((s, e) => { room.Update(tick); });
			//↑↑시스템 타이머를 사용하면 메인쓰레드가아닌 워커쓰레드에서 실행될 수 있다.↑↑
			timer.AutoReset = true;
			timer.Enabled = true;

			_timers.Add(timer);
		}

		static void Main(string[] args)
		{
			ConfigManager.LoadConfig();
			DataManager.LoadData();

			GameRoom Room = RoomManager.Instance.CreateAndAddRoom(1);//여기서 ID가 1번인 GameRoom이 생성된다.
			TickRoom(Room, 50);
			
			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, SessionManager.Instance.GenerateClientSession);
			//↑리스너 init시에 클라이언트에서 접속 요청이 왔을때 세션을 제너레이트해줄 함수를 등록해야한다. 

			Console.WriteLine("Listening...");

			//FlushRoom();
			//JobTimer.Instance.Push(FlushRoom);

			while (true)
			{
				//DbTransction.Instance.Flush();
				Thread.Sleep(100);
			}
		}
	}
}
