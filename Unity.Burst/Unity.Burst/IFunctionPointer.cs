using System;

namespace Unity.Burst;

public interface IFunctionPointer
{
	[Obsolete("This method will be removed in a future version of Burst")]
	IFunctionPointer FromIntPtr(IntPtr ptr);
}
