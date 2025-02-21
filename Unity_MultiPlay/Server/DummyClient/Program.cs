// DNS (Domain Name System)
using DummyClient.Session;
using ServerCore;
using System.Net;

Thread.Sleep(2000);

string host = Dns.GetHostName();
IPHostEntry ipHost = Dns.GetHostEntry(host);
IPAddress ipAddr = ipHost.AddressList[1];
IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

Connector connector = new Connector();

connector.Connect(endPoint, SessionManager.Instance.GenerateSession, SessionManager.DummyClientCount);

while (true)
{
	Thread.Sleep(1000);
}