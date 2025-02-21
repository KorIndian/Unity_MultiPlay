using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
	class SessionManager
	{
		static SessionManager _session = new SessionManager();
		public static SessionManager Instance { get { return _session; } }

		//TODO 메인에서 타이머로 실시간 Session카운트를 보면서 Listner한테 listen하고 있을건지 말건지 명령해야함.
		public const int MaxSessionCount = 500; 
		public const int CrowdedLevelStride = 100;

		private int _sessionId = 0;
		Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();
		object _lock = new object();

		public int GetCrowdedLevel()
		{
			int CrowdedLevel = 0;
			lock (_lock)
			{
				CrowdedLevel = _sessions.Count / CrowdedLevelStride;
			}
			return CrowdedLevel;
		}

		public List<ClientSession> GetSessions()
		{
			List<ClientSession> clientSessions = new List<ClientSession>();
			lock (_lock)
			{
				clientSessions.AddRange(_sessions.Values);
			}
			return clientSessions;
		}

		public ClientSession GenerateClientSession()
		{
			lock (_lock)
			{
				int sessionId = ++_sessionId;

				ClientSession session = new ClientSession();
				session.SessionId = sessionId;
				_sessions.Add(sessionId, session);

				Console.WriteLine($"GenerateClientSession : Connected ({_sessions.Count})Players");

				return session;
			}
		}

		public ClientSession FindSession(int id)
		{
			lock (_lock)
			{
				ClientSession session = null;
				_sessions.TryGetValue(id, out session);
				return session;
			}
		}

		public void RemoveSession(ClientSession session)
		{
			lock (_lock)
			{
				_sessions.Remove(session.SessionId);
				Console.WriteLine($"RemoveSession : Connected ({_sessions.Count})Players");
			}
		}
	}
}
