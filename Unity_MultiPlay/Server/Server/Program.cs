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
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DB;
using Server.GameContents;
using Server.Http;
using ServerCore;
using static System.Net.Mime.MediaTypeNames;

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

		static void HttpProcess(HttpListenerContext context)
		{
			try
			{
				string receivedtext;
				using (StreamReader reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
				{
					receivedtext = reader.ReadToEnd();
				}

				string responceText = "Something";
				using (Stream output = context.Response.OutputStream)
				{
					using (StreamWriter writer = new StreamWriter(output) { AutoFlush = true })
					{
						writer.Write(responceText);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		static async void HttpTestClient()
		{
			HttpClient client = new HttpClient();
			HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "Some URL");
			httpRequestMessage.Content = new StringContent("Some Json");
			HttpResponseMessage result = await client.SendAsync(httpRequestMessage);

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
