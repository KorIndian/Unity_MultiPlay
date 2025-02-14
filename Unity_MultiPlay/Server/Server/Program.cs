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
	//기능별로 쓰레드 분할.
	//1. GameLogic 전용(1개)
	//2. Recv (N개)
	//3. DB 전용(1개)
	//4. Send 전용(1개)
	class Program
	{
		static Listener _listener = new Listener();
		
		static void GameLogicTask()
		{
			while (true)
			{
				GameLogic.Instance.Update();
			}
		}

		static void DbTask()
		{
			while (true)
			{
				DbTransaction.Instance.FlushJobs();
				Thread.Sleep(0);//while루프 끝에 슬립을 한번씩 호출해주면 커널이 소유권을 잠시 넘겨받게되어,
				//계속 쓰레드를 점유하는 것이 아닌, 논리코어처럼 동작하게된다. 
			}
		}

		static void NetworkTask()
		{
			while (true)
			{
				var sessions = SessionManager.Instance.GetSessions();
				foreach (var session in sessions)
				{
					session.FlushSend();
				}
				Thread.Sleep(0);
			}
		}

		static void Main(string[] args)
		{
			ConfigManager.LoadConfig();
			DataManager.LoadData();

			//여기서 ID가 1번인 GameRoom이 생성된다.
			GameLogic.Instance.PushJob(() => { GameLogic.Instance.CreateAndAddRoom(1); });

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, SessionManager.Instance.GenerateClientSession);
			//↑리스너 init시에 클라이언트에서 접속 요청이 왔을때 세션을 제너레이트해줄 함수를 등록해야한다. 
			Console.WriteLine("Listening...");

			{//GameLogic 전용 쓰레드 생성.
				Task gameLogicTask = new Task(GameLogicTask, TaskCreationOptions.LongRunning);
				gameLogicTask.Start();
			}

			{//GameLogic 전용 쓰레드 생성.
				Task netWorkTask = new Task(NetworkTask, TaskCreationOptions.LongRunning);
				netWorkTask.Start();
			}

			DbTask();//메인 쓰레드 살려두는용도.
		}
	}
}
