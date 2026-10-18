using System;
using System.Collections.Generic;
using System.Threading;

namespace g3;

public class ParallelStream<V, T>
{
	public Func<V, T> ProducerF;

	public Action<T> ConsumerF;

	private LockingQueue<T> store0 = new LockingQueue<T>();

	private IEnumerable<V> source;

	private bool producer_done;

	private AutoResetEvent consumer_done_event;

	public void Run_NoThreads(IEnumerable<V> sourceIn)
	{
		foreach (V item in sourceIn)
		{
			T obj = ProducerF(item);
			ConsumerF(obj);
		}
	}

	public void Run(IEnumerable<V> sourceIn)
	{
		source = sourceIn;
		producer_done = false;
		consumer_done_event = new AutoResetEvent(initialState: false);
		Thread thread = new Thread(ProducerThreadFunc);
		thread.Name = "ParallelStream_producer";
		thread.Start();
		Thread thread2 = new Thread(ConsumerThreadFunc);
		thread2.Name = "ParallelStream_consumer";
		thread2.Start();
		consumer_done_event.WaitOne();
	}

	private void ProducerThreadFunc()
	{
		foreach (V item in source)
		{
			T obj = ProducerF(item);
			store0.Add(obj);
		}
		producer_done = true;
	}

	private void ConsumerThreadFunc()
	{
		T val = default(T);
		while (!producer_done || store0.Count > 0)
		{
			if (store0.Remove(ref val))
			{
				ConsumerF(val);
			}
		}
		consumer_done_event.Set();
	}
}
