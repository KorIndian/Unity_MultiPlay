using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DummyClient.Session;

public class SessionManager
{
	public static SessionManager Instance { get; } = new();

	public static int DummyClientCount { get; private set; } = 500;

	HashSet<ServerSession> _sessions = new HashSet<ServerSession>();

	object _lock = new object();
	int _dummyId = 1;

	public ServerSession GenerateSession()
	{
		lock (_lock)
		{
			ServerSession session = new ServerSession();
			session.DummyId = _dummyId;
			_dummyId++;

			_sessions.Add(session);
			Console.WriteLine($"GenerateSession : Connected ({_sessions.Count})Players");
			return session;
		}
	}

	public void RemoveSession(ServerSession session)
	{
		lock (_lock)
		{
			_sessions.Remove(session);
			Console.WriteLine($"RemoveSession : Connected ({_sessions.Count})Players");
		}
	}
}
