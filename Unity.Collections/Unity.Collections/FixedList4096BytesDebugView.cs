namespace Unity.Collections;

internal sealed class FixedList4096BytesDebugView<T> where T : unmanaged
{
	private FixedList4096Bytes<T> m_List;

	public T[] Items => m_List.ToArray();

	public FixedList4096BytesDebugView(FixedList4096Bytes<T> list)
	{
		m_List = list;
	}
}
