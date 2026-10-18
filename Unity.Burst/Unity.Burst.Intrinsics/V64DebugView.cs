using System.Diagnostics;

namespace Unity.Burst.Intrinsics;

internal class V64DebugView
{
	private v64 m_Value;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public byte[] Byte => new byte[8] { m_Value.Byte0, m_Value.Byte1, m_Value.Byte2, m_Value.Byte3, m_Value.Byte4, m_Value.Byte5, m_Value.Byte6, m_Value.Byte7 };

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public sbyte[] SByte => new sbyte[8] { m_Value.SByte0, m_Value.SByte1, m_Value.SByte2, m_Value.SByte3, m_Value.SByte4, m_Value.SByte5, m_Value.SByte6, m_Value.SByte7 };

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public ushort[] UShort => new ushort[4] { m_Value.UShort0, m_Value.UShort1, m_Value.UShort2, m_Value.UShort3 };

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public short[] SShort => new short[4] { m_Value.SShort0, m_Value.SShort1, m_Value.SShort2, m_Value.SShort3 };

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public uint[] UInt => new uint[2] { m_Value.UInt0, m_Value.UInt1 };

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public int[] SInt => new int[2] { m_Value.SInt0, m_Value.SInt1 };

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public float[] Float => new float[2] { m_Value.Float0, m_Value.Float1 };

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public long[] SLong => new long[1] { m_Value.SLong0 };

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public ulong[] ULong => new ulong[1] { m_Value.ULong0 };

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public double[] Double => new double[1] { m_Value.Double0 };

	public V64DebugView(v64 value)
	{
		m_Value = value;
	}
}
