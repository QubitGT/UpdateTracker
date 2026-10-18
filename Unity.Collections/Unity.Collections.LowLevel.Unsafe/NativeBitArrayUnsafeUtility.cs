namespace Unity.Collections.LowLevel.Unsafe;

[GenerateTestsForBurstCompatibility]
public static class NativeBitArrayUnsafeUtility
{
	public unsafe static NativeBitArray ConvertExistingDataToNativeBitArray(void* ptr, int sizeInBytes, AllocatorManager.AllocatorHandle allocator)
	{
		UnsafeBitArray* ptr2 = UnsafeBitArray.Alloc(Allocator.Persistent);
		*ptr2 = new UnsafeBitArray(ptr, sizeInBytes, allocator);
		NativeBitArray result = default(NativeBitArray);
		result.m_BitArray = ptr2;
		result.m_Allocator = Allocator.Persistent;
		return result;
	}
}
