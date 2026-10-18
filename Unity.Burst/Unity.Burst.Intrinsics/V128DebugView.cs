using System.Diagnostics;

namespace Unity.Burst.Intrinsics;

internal class V128DebugView
{
	private v128 m_Value;

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public byte[] Byte => new byte[16]
	{
		m_Value.Byte0, m_Value.Byte1, m_Value.Byte2, m_Value.Byte3, m_Value.Byte4, m_Value.Byte5, m_Value.Byte6, m_Value.Byte7, m_Value.Byte8, m_Value.Byte9,
		m_Value.Byte10, m_Value.Byte11, m_Value.Byte12, m_Value.Byte13, m_Value.Byte14, m_Value.Byte15
	};

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public sbyte[] SByte => new sbyte[16]
	{
		m_Value.SByte0, m_Value.SByte1, m_Value.SByte2, m_Value.SByte3, m_Value.SByte4, m_Value.SByte5, m_Value.SByte6, m_Value.SByte7, m_Value.SByte8, m_Value.SByte9,
		m_Value.SByte10, m_Value.SByte11, m_Value.SByte12, m_Value.SByte13, m_Value.SByte14, m_Value.SByte15
	};

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public ushort[] UShort => new ushort[8] { m_Value.UShort0, m_Value.UShort1, m_Value.UShort2, m_Value.UShort3, m_Value.UShort4, m_Value.UShort5, m_Value.UShort6, m_Value.UShort7 };

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public short[] SShort => new short[8] { m_Value.SShort0, m_Value.SShort1, m_Value.SShort2, m_Value.SShort3, m_Value.SShort4, m_Value.SShort5, m_Value.SShort6, m_Value.SShort7 };

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public uint[] UInt => new uint[4] { m_Value.UInt0, m_Value.UInt1, m_Value.UInt2, m_Value.UInt3 };

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public int[] SInt => new int[4] { m_Value.SInt0, m_Value.SInt1, m_Value.SInt2, m_Value.SInt3 };

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public float[] Float => new float[4] { m_Value.Float0, m_Value.Float1, m_Value.Float2, m_Value.Float3 };

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public long[] SLong => new long[2] { m_Value.SLong0, m_Value.SLong1 };

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public ulong[] ULong => new ulong[2] { m_Value.ULong0, m_Value.ULong1 };

	[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
	public double[] Double => new double[2] { m_Value.Double0, m_Value.Double1 };

	public V128DebugView(v128 value)
	{
		m_Value = value;
	}
}
