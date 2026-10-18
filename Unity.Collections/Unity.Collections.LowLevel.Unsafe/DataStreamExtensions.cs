namespace Unity.Collections.LowLevel.Unsafe;

[GenerateTestsForBurstCompatibility]
public static class DataStreamExtensions
{
	public unsafe static DataStreamWriter Create(byte* data, int length)
	{
		return new DataStreamWriter(NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(data, length, Allocator.None));
	}

	public unsafe static bool WriteBytesUnsafe(this ref DataStreamWriter writer, byte* data, int bytes)
	{
		NativeArray<byte> value = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(data, bytes, Allocator.None);
		return writer.WriteBytes(value);
	}

	public unsafe static void ReadBytesUnsafe(this ref DataStreamReader reader, byte* data, int length)
	{
		NativeArray<byte> array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(data, length, Allocator.None);
		reader.ReadBytes(array);
	}

	public unsafe static ushort ReadFixedStringUnsafe(this ref DataStreamReader reader, byte* data, int maxLength)
	{
		NativeArray<byte> array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(data, maxLength, Allocator.Temp);
		return reader.ReadFixedString(array);
	}

	public unsafe static ushort ReadPackedFixedStringDeltaUnsafe(this ref DataStreamReader reader, byte* data, int maxLength, byte* baseData, ushort baseLength, StreamCompressionModel model)
	{
		NativeArray<byte> data2 = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(data, maxLength, Allocator.Temp);
		NativeArray<byte> baseData2 = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(baseData, baseLength, Allocator.Temp);
		return reader.ReadPackedFixedStringDelta(data2, baseData2, in model);
	}

	public unsafe static void* GetUnsafeReadOnlyPtr(this ref DataStreamReader reader)
	{
		return reader.m_BufferPtr;
	}
}
