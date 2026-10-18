namespace Unity.Collections;

public interface IUTF8Bytes
{
	bool IsEmpty { get; }

	unsafe byte* GetUnsafePtr();

	bool TryResize(int newLength, NativeArrayOptions clearOptions = NativeArrayOptions.ClearMemory);
}
