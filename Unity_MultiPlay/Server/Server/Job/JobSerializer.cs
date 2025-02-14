using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class JobSerializer
    {
        JobTimer _timer = new JobTimer();
		protected Queue<IJob> _jobQueue = new Queue<IJob>();
        object _lock = new object();

        public virtual void PushJob(Action action)
        {
            PushJob(new Job(action));
        }

        public virtual void PushJob<T1>(Action<T1> action, T1 t1)
        {
            PushJob(new Job<T1>(action, t1));
        }

        public virtual void PushJob<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2)
        {
            PushJob(new Job<T1, T2>(action, t1, t2));
        }

        public virtual void PushJob<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3)
        {
            PushJob(new Job<T1, T2, T3>(action, t1, t2 ,t3));
        }

        public IJob PushAfter(Action action, int TickAfter)
        {
            return PushAfter(new Job(action), TickAfter);
        }

        public IJob PushAfter<T1>(Action<T1> action, T1 t1, int TickAfter)
        {
			return PushAfter(new Job<T1>(action, t1), TickAfter);
        }

        public IJob PushAfter<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2, int TickAfter)
        {
			return PushAfter(new Job<T1, T2>(action, t1, t2), TickAfter);
        }

        public IJob PushAfter<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3, int TickAfter)
        {
			return PushAfter(new Job<T1, T2, T3>(action, t1, t2, t3), TickAfter);
        }

        public IJob PushAfter(IJob job, int TickAfter)
        {
			_timer.Push(job, TickAfter);
            return job;
		}

        public void PushJob(IJob job)
        {
            lock (_lock)
            {
                _jobQueue.Enqueue(job);
            }
        }

        public void FlushJobs()
        {
            _timer.Flush();

            while (true)
            {
                IJob job = Pop();
                if (job == null)
                    return;

                job.Excute();
            }
        }

        IJob Pop()
        {
            lock (_lock)
            {
                if (_jobQueue.Count == 0)
                {
                    return null;
                }
                return _jobQueue.Dequeue();
            }
        }
    }
}
