using System;

namespace Liv.Lck.Encoding;

internal struct LckEncodedPacketCallback
{
	public IntPtr CallbackObjectPtr { get; set; }

	public IntPtr CallbackFunctionPtr { get; set; }

	public bool IsValid
	{
		get
		{
			if (CallbackObjectPtr != IntPtr.Zero)
			{
				return CallbackFunctionPtr != IntPtr.Zero;
			}
			return false;
		}
	}

	public LckEncodedPacketCallback(IntPtr callbackObjectPtr, IntPtr callbackFunctionPtr)
	{
		CallbackObjectPtr = callbackObjectPtr;
		CallbackFunctionPtr = callbackFunctionPtr;
	}
}
