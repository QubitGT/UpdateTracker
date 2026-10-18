using System;
using System.Runtime.InteropServices;

namespace Liv.NGFX;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ResourceDestroyInfo
{
	public static EventType eventType = EventType.ResourceDestroy;

	private IntPtr m_context;

	private uint m_id;

	public ResourceDestroyInfo(IntPtr ctx, uint id)
	{
		m_context = ctx;
		m_id = id;
	}
}
