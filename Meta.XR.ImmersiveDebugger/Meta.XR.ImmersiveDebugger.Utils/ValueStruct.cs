using System;

namespace Meta.XR.ImmersiveDebugger.Utils;

[Serializable]
internal struct ValueStruct<T>
{
	public string ValueName;

	public T Value;
}
