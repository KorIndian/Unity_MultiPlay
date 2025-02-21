using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using CommonWebPacket;

public class NetworkManager : IDisposable
{
	ServerSession _session = new ServerSession();

	public string AccountName { get; private set; } = string.Empty;
	public int LoginToken { get; private set; }

	public void Send(IMessage packet)
	{
		_session.Send(packet);
	}

	public void SetLoginAccountName(string accountName)
	{
		AccountName = accountName;
	}

	public void SetLoginToken(int token)
	{
		LoginToken = token;
	}

	public void ConnectToGameServer(ServerStatus serverStatus)
	{
        PacketManager.Instance.CustomHandler = (s, m, i) =>
        {
            PacketQueue.Instance.Push(i, m);
			//패킷이 오면 바로 처리하려고 하는 것이 아니라 패킷큐에 담아 뒀다가 처리하도록.
			//이렇게 하면 소켓의 워커쓰레드가 아니라 게임쓰레드에서 주기적으로 핸들러가 처리되기 때문에
			//로직이 동기화되면서 크래쉬위험이 줄어든다. 단, 게임쓰레드가 무거워진다.
        };
		
		IPAddress ipAddr = IPAddress.Parse(serverStatus.IpAddress);
		IPEndPoint endPoint = new IPEndPoint(ipAddr, serverStatus.Port);

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
