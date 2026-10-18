namespace Unity.Collections;

internal sealed class FixedList32BytesDebugView<T> where T : unmanaged
{
	private FixedList32Bytes<T> m_List;

	public T[] Items => m_List.ToArray();

	public FixedList32BytesDebugView(FixedList32Bytes<T> list)
	{
		m_List = list;
	}
}
