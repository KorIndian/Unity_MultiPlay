using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonWebPacket;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DB;
using Server.GameContents;
using Server.Http;
using ServerCore;
using SharedDB;
using static System.Net.Mime.MediaTypeNames;
using static SharedDB.DataModel;

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
				//Thread.Sleep(0);
			}
		}

		static void DbTask()
		{
			while (true)
			{
				DbTransaction.Instance.FlushJobs();
				//Thread.Sleep(0);
			}
		}

		static void NetworkSendTask()
		{
			while (true)
			{
				var sessions = SessionManager.Instance.GetSessions();
				
				foreach (var session in sessions)
				{
					session.FlushSend();
				}
				//Thread.Sleep(0);
			}
		}

		static void HttpServerTask()
		{
			HttpServer.Instance.Start();

			while (true)
			{
				HttpListenerContext context = HttpServer.Instance.listener.GetContext();
				Task.Run(() => 
				{
					HttpServer.Instance.HandleContext(context);
				});
			}
		}

		static void StartServerInfoTask()
		{
			var t = new System.Timers.Timer();
			t.AutoReset = true;
			t.Elapsed += new System.Timers.ElapsedEventHandler((sender, eArgs) =>
			{
				using (SharedDbContext shared = new SharedDbContext())
				{
					ServerStatusDb serverStatus = shared.Servers.Where(s => s.Name == Program.ServerName).FirstOrDefault();
					if(serverStatus != null)
					{
						serverStatus.Name = Program.ServerName;
						serverStatus.IpAddress = Program.IpAddress;
						serverStatus.Port = Program.Port;
						serverStatus.CrowdedLevel = SessionManager.Instance.GetCrowdedLevel();
					}
					else
					{
						serverStatus = new ServerStatusDb()
						{
							Name = Program.ServerName,
							IpAddress = Program.IpAddress,
							Port = Program.Port,
							CrowdedLevel = SessionManager.Instance.GetCrowdedLevel()
						};
						shared.Add(serverStatus);
					}
					shared.SaveChangesEx();
				}
			});
			t.Interval = 10 * 1000;
			t.Start();
		}

		public static string ServerName { get; } = "GameServer1";
		public static int Port { get; } = 7777;
		public static string IpAddress { get; set; }

		static void Main(string[] args)
		{
			ConfigManager.LoadConfig();
			DataManager.LoadData();

			//여기서 ID가 1번인 GameRoom이 생성된다.
			GameLogic.Instance.PushJob(() => { GameLogic.Instance.CreateAndAddRoom(1); });

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[1];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, Port);

			IpAddress = ipAddr.ToString();

			_listener.Init(endPoint, SessionManager.Instance.GenerateClientSession);
			//↑리스너 init시에 클라이언트에서 접속 요청이 왔을때 세션을 제너레이트해줄 함수를 등록해야한다. 
			Console.WriteLine("Listening...");

			StartServerInfoTask();

			{//HttpThread 전용 쓰레드 생성.
				Thread HttpThread = new Thread(HttpServerTask);
				HttpThread.Name = "HttpThread";
				HttpThread.Start();
			}

			{//DbTask 전용 쓰레드 생성.
				Thread DbThread = new Thread(DbTask);
				DbThread.Name = "DbThread";
				DbThread.Start();
			}
			
			{//NetWork 전용 쓰레드 생성.
				Thread NetworkSendThread = new Thread(NetworkSendTask);
				NetworkSendThread.Name = "NetworkSendThread";
				NetworkSendThread.Start();
			}

			GameLogicTask();//게임 로직은 메인쓰레드가 담당.
		}
	}
}
