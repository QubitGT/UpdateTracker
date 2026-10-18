namespace Unity.Collections;

internal sealed class FixedList512BytesDebugView<T> where T : unmanaged
{
	private FixedList512Bytes<T> m_List;

	public T[] Items => m_List.ToArray();

	public FixedList512BytesDebugView(FixedList512Bytes<T> list)
	{
		m_List = list;
	}
}
