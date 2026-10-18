using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.Rendering;

internal struct InstanceOcclusionEventDebugArray : IDisposable
{
	internal struct Info
	{
		public int viewInstanceID;

		public InstanceOcclusionEventType eventType;

		public int occluderVersion;

		public int subviewMask;

		public OcclusionTest occlusionTest;

		public bool HasVersion()
		{
			if (eventType != InstanceOcclusionEventType.OccluderUpdate)
			{
				return occlusionTest != OcclusionTest.None;
			}
			return true;
		}
	}

	internal struct Request
	{
		public UnsafeList<Info> info;

		public AsyncGPUReadbackRequest readback;
	}

	private const int InitialPassCount = 4;

	private const int MaxPassCount = 64;

	private GraphicsBuffer m_CounterBuffer;

	private UnsafeList<Info> m_PendingInfo;

	private NativeQueue<Request> m_Requests;

	private UnsafeList<Info> m_LatestInfo;

	private NativeArray<int> m_LatestCounters;

	private bool m_HasLatest;

	public GraphicsBuffer CounterBuffer => m_CounterBuffer;

	public void Init()
	{
		m_CounterBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 256, 4);
		m_PendingInfo = new UnsafeList<Info>(4, Allocator.Persistent);
		m_Requests = new NativeQueue<Request>(Allocator.Persistent);
	}

	public void Dispose()
	{
		if (m_HasLatest)
		{
			m_LatestInfo.Dispose();
			m_LatestCounters.Dispose();
			m_HasLatest = false;
		}
		Request item;
		while (m_Requests.TryDequeue(out item))
		{
			item.readback.WaitForCompletion();
			item.info.Dispose();
		}
		m_Requests.Dispose();
		m_PendingInfo.Dispose();
		m_CounterBuffer.Dispose();
	}

	public int TryAdd(int viewInstanceID, InstanceOcclusionEventType eventType, int occluderVersion, int subviewMask, OcclusionTest occlusionTest)
	{
		int length = m_PendingInfo.Length;
		if (length + 1 > 64)
		{
			return -1;
		}
		ref UnsafeList<Info> pendingInfo = ref m_PendingInfo;
		Info value = new Info
		{
			viewInstanceID = viewInstanceID,
			eventType = eventType,
			occluderVersion = occluderVersion,
			subviewMask = subviewMask,
			occlusionTest = occlusionTest
		};
		pendingInfo.Add(in value);
		return length;
	}

	public void MoveToDebugStatsAndClear(DebugRendererBatcherStats debugStats)
	{
		if (m_PendingInfo.Length > 0)
		{
			m_Requests.Enqueue(new Request
			{
				info = m_PendingInfo,
				readback = AsyncGPUReadback.Request(m_CounterBuffer, m_PendingInfo.Length * 4 * 4, 0)
			});
			m_PendingInfo = new UnsafeList<Info>(4, Allocator.Persistent);
		}
		while (!m_Requests.IsEmpty() && m_Requests.Peek().readback.done)
		{
			Request request = m_Requests.Dequeue();
			if (request.readback.hasError)
			{
				continue;
			}
			NativeArray<int> data = request.readback.GetData<int>();
			if (data.Length == request.info.Length * 4)
			{
				if (m_HasLatest)
				{
					m_LatestInfo.Dispose();
					m_LatestCounters.Dispose();
					m_HasLatest = false;
				}
				m_LatestInfo = request.info;
				m_LatestCounters = new NativeArray<int>(data, Allocator.Persistent);
				m_HasLatest = true;
			}
		}
		debugStats.instanceOcclusionEventStats.Clear();
		if (m_HasLatest)
		{
			for (int i = 0; i < m_LatestInfo.Length; i++)
			{
				Info info = m_LatestInfo[i];
				int occluderVersion = -1;
				if (info.HasVersion())
				{
					occluderVersion = 0;
					for (int j = 0; j < i; j++)
					{
						Info info2 = m_LatestInfo[j];
						if (info2.HasVersion() && info2.viewInstanceID == info.viewInstanceID)
						{
							occluderVersion = info.occluderVersion - info2.occluderVersion;
							break;
						}
					}
				}
				int num = i * 4;
				int culledInstances = m_LatestCounters[num];
				int visibleInstances = m_LatestCounters[num + 1];
				int culledPrimitives = m_LatestCounters[num + 2];
				int visiblePrimitives = m_LatestCounters[num + 3];
				ref NativeList<InstanceOcclusionEventStats> instanceOcclusionEventStats = ref debugStats.instanceOcclusionEventStats;
				InstanceOcclusionEventStats value = new InstanceOcclusionEventStats
				{
					viewInstanceID = info.viewInstanceID,
					eventType = info.eventType,
					occluderVersion = occluderVersion,
					subviewMask = info.subviewMask,
					occlusionTest = info.occlusionTest,
					visibleInstances = visibleInstances,
					culledInstances = culledInstances,
					visiblePrimitives = visiblePrimitives,
					culledPrimitives = culledPrimitives
				};
				instanceOcclusionEventStats.Add(in value);
			}
		}
		NativeArray<int> data2 = new NativeArray<int>(256, Allocator.Temp);
		m_CounterBuffer.SetData(data2);
		data2.Dispose();
	}
}
