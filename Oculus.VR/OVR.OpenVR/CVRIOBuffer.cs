using System;
using System.Runtime.InteropServices;

namespace OVR.OpenVR;

public class CVRIOBuffer
{
	private IVRIOBuffer FnTable;

	internal CVRIOBuffer(IntPtr pInterface)
	{
		FnTable = (IVRIOBuffer)Marshal.PtrToStructure(pInterface, typeof(IVRIOBuffer));
	}

	public EIOBufferError Open(string pchPath, EIOBufferMode mode, uint unElementSize, uint unElements, ref ulong pulBuffer)
	{
		pulBuffer = 0uL;
		return FnTable.Open(pchPath, mode, unElementSize, unElements, ref pulBuffer);
	}

	public EIOBufferError Close(ulong ulBuffer)
	{
		return FnTable.Close(ulBuffer);
	}

	public EIOBufferError Read(ulong ulBuffer, IntPtr pDst, uint unBytes, ref uint punRead)
	{
		punRead = 0u;
		return FnTable.Read(ulBuffer, pDst, unBytes, ref punRead);
	}

	public EIOBufferError Write(ulong ulBuffer, IntPtr pSrc, uint unBytes)
	{
		return FnTable.Write(ulBuffer, pSrc, unBytes);
	}

	public ulong PropertyContainer(ulong ulBuffer)
	{
		return FnTable.PropertyContainer(ulBuffer);
	}
}
