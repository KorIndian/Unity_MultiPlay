using CommonWebPacket;
using Newtonsoft.Json;
using Server.GameContents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server.Http;

public class HttpServer
{
	public static HttpServer Instance { get; set; } = new HttpServer();

	public HttpListener listener { get; private set; } = new HttpListener();
	Dictionary<string, Func<string, string>> receiveHandler = new Dictionary<string, Func<string, string>>();

	public HttpServer()
	{
		receiveHandler.Add(nameof(LoginAccountPacketReq), HttpReceiveHandler.HandleLoginAccountReq);
		receiveHandler.Add(nameof(QueryServerStatusReq), HttpReceiveHandler.HandleQueryServerStatusReq);
	}

	public void Start()
	{
		listener.Prefixes.Add(String.Format("http://localhost:{0}/", 7778));//TODO File로 관리
		listener.Start();
	}

	public void HandleContext(HttpListenerContext context)
	{
		try
		{
			string receivedtext;
			using (StreamReader reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
			{
				receivedtext = reader.ReadToEnd();
				string handlerKey = context.Request.Url.OriginalString.Split('/').Last();
				string responseText = "";
				if(receiveHandler.TryGetValue(handlerKey, out var action))
				{
					responseText = action.Invoke(receivedtext);
					using (Stream output = context.Response.OutputStream)
					{
						using (StreamWriter writer = new StreamWriter(output) { AutoFlush = true })
						{
							writer.Write(responseText);
						}
					}
				}

			}

		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
		}
	}
}

public static class HttpReceiveHandler
{
	public static string HandleLoginAccountReq(string receivedtext)
	{
		string responseText = "";
		var packet = JsonConvert.DeserializeObject<LoginAccountPacketReq>(receivedtext);
		if (packet == null)
			return responseText;
		Console.WriteLine($"Login Requested Id : {packet.AccountName}");

		
		return responseText;
	}

	public static string HandleQueryServerStatusReq(string receivedtext)
	{
		var packet = JsonConvert.DeserializeObject<QueryServerStatusReq>(receivedtext);
		if (packet == null)
			return "" ;
		Console.WriteLine($"QuryReq data : {packet.data}");

		QueryServerStatusRes serverStatusRes = new QueryServerStatusRes() { data = "ServerState Response" };

		return JsonConvert.SerializeObject(serverStatusRes);
	}
}
