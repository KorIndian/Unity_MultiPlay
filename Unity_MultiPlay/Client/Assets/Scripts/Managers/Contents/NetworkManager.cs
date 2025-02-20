using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Google.Protobuf;
using Google.Protobuf.Protocol;

public class NetworkManager : IDisposable
{
	ServerSession _session = new ServerSession();

	public void Send(IMessage packet)
	{
		_session.Send(packet);
	}

	public void ConnectToGameServer()
	{
        PacketManager.Instance.CustomHandler = (s, m, i) =>
        {
            PacketQueue.Instance.Push(i, m);
			//패킷이 오면 바로 처리하려고 하는 것이 아니라 패킷큐에 담아 뒀다가 처리하도록.
			//이렇게 하면 소켓의 워커쓰레드가 아니라 게임쓰레드에서 주기적으로 핸들러가 처리되기 때문에
			//로직이 동기화되면서 크래쉬위험이 줄어든다. 단, 게임쓰레드가 무거워진다.
        };
		// DNS (Domain Name System)
		string host = Dns.GetHostName();
		IPHostEntry ipHost = Dns.GetHostEntry(host);
		IPAddress ipAddr = ipHost.AddressList[0];
		IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

		Connector connector = new Connector();

		connector.Connect(endPoint,
			() => { return _session; },
			1);
	}

	public void Update()
	{
		List<PacketMessage> list = PacketQueue.Instance.PopAll();
		foreach (PacketMessage packet in list)
		{
			Action<PacketSession, IMessage> handler = PacketManager.Instance.GetPacketHandler(packet.Id);
			if (handler != null)
				handler.Invoke(_session, packet.Message);
		}	
	}

	public void Dispose()
	{
		_session.Disconnect();
		_session = null;
	}
}
