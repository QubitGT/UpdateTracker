using System;

namespace Fusion;

public struct NetworkObjectHeaderPtr
{
	public unsafe NetworkObjectHeader* Ptr;

	public unsafe NetworkObjectTypeId Type => Ptr->Type;

	public unsafe NetworkId Id => Ptr->Id;

	public unsafe Span<int> Data => new Span<int>((byte*)Ptr + (nint)20 * (nint)4, Ptr->WordCount - 20);

	public unsafe NetworkObjectHeaderPtr(NetworkObjectHeader* ptr)
	{
		Ptr = ptr;
	}
}
