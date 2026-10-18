namespace Unity.Collections;

internal sealed class FixedList128BytesDebugView<T> where T : unmanaged
{
	private FixedList128Bytes<T> m_List;

	public T[] Items => m_List.ToArray();

	public FixedList128BytesDebugView(FixedList128Bytes<T> list)
	{
		m_List = list;
	}
}
