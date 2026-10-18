namespace Unity.Collections.LowLevel.Unsafe;

[GenerateTestsForBurstCompatibility]
internal struct UnsafeStreamBlock
{
	internal unsafe UnsafeStreamBlock* Next;

	internal unsafe fixed byte Data[1];
}
