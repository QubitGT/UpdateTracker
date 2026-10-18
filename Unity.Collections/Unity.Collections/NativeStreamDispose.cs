using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

[NativeContainer]
[GenerateTestsForBurstCompatibility]
internal struct NativeStreamDispose
{
	public UnsafeStream m_StreamData;

	public void Dispose()
	{
		m_StreamData.Dispose();
	}
}
