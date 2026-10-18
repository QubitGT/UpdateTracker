using System.Runtime.InteropServices;

namespace UnityEngine.Rendering;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct EditorInstanceDataArrays : IDataArrays
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal readonly struct ReadOnly
	{
		public ReadOnly(in CPUInstanceData instanceData)
		{
		}
	}

	public void Initialize(int initCapacity)
	{
	}

	public void Dispose()
	{
	}

	public void Grow(int newCapacity)
	{
	}

	public void Remove(int index, int lastIndex)
	{
	}

	public void SetDefault(int index)
	{
	}
}
