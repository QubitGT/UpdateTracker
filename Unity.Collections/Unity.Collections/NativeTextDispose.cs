using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

[NativeContainer]
[GenerateTestsForBurstCompatibility]
internal struct NativeTextDispose
{
	[NativeDisableUnsafePtrRestriction]
	public unsafe UnsafeText* m_TextData;

	public unsafe void Dispose()
	{
		UnsafeText.Free(m_TextData);
	}
}
