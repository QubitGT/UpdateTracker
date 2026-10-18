namespace UnityEngine.Rendering.Universal.UTess;

internal sealed class ArraySliceDebugView<T> where T : struct
{
	private ArraySlice<T> m_Slice;

	public T[] Items => m_Slice.ToArray();

	public ArraySliceDebugView(ArraySlice<T> slice)
	{
		m_Slice = slice;
	}
}
