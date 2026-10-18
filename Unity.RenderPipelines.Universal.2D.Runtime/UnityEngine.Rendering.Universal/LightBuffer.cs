using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.Rendering.Universal;

internal class LightBuffer
{
	internal static readonly int kMax = 16384;

	internal static readonly int kCount = 1;

	internal static readonly int kLightMod = 64;

	internal static readonly int kBatchMax = 256;

	private GraphicsBuffer m_GraphicsBuffer;

	private NativeArray<int> m_Markers = new NativeArray<int>(kBatchMax, Allocator.Persistent);

	private NativeArray<PerLight2D> m_NativeBuffer = new NativeArray<PerLight2D>(kMax, Allocator.Persistent);

	internal GraphicsBuffer graphicsBuffer
	{
		get
		{
			if (m_GraphicsBuffer == null)
			{
				m_GraphicsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, kMax, UnsafeUtility.SizeOf<PerLight2D>());
			}
			return m_GraphicsBuffer;
		}
	}

	internal NativeArray<int> lightMarkers => m_Markers;

	internal NativeArray<PerLight2D> nativeBuffer => m_NativeBuffer;

	internal void Release()
	{
		m_GraphicsBuffer.Release();
		m_GraphicsBuffer = null;
	}

	internal unsafe void Reset()
	{
		UnsafeUtility.MemClear(m_Markers.GetUnsafePtr(), UnsafeUtility.SizeOf<int>() * kBatchMax);
		UnsafeUtility.MemClear(m_NativeBuffer.GetUnsafePtr(), UnsafeUtility.SizeOf<PerLight2D>() * kMax);
	}
}
