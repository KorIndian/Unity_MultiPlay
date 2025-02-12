using System;
using System.Collections.Generic;
using System.Text;
using ServerCore;

namespace Server
{
	struct JobTimerElem : IComparable<JobTimerElem>
	{
		public int execTick; // 실행 시간
		public IJob job;

		public int CompareTo(JobTimerElem other)
		{
			return other.execTick - execTick;
		}
	}

	public class JobTimer
	{
		PriorityQueue<JobTimerElem> _pq = new PriorityQueue<JobTimerElem>();
		object _lock = new object();

		public void Push(IJob job, int tickAfter = 0)
		{
			JobTimerElem jobElem;
			jobElem.execTick = System.Environment.TickCount + tickAfter;
			jobElem.job = job;

			lock (_lock)
			{
				_pq.Push(jobElem);
			}
		}

		public void Flush()
		{
			while (true)
			{
				int now = System.Environment.TickCount;

				JobTimerElem jobElem;

				lock (_lock)
				{
					if (_pq.Count == 0)
						break;

					jobElem = _pq.Peek();//제일 위에있는게 아직 시간이 안됬으면 그냥 빠져나간다.
					if (jobElem.execTick > now)
						break;

					_pq.Pop();
				}

				jobElem.job.Excute();
			}
		}
	}
}
