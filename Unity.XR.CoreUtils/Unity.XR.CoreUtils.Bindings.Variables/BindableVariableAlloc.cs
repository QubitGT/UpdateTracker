using System;

namespace Unity.XR.CoreUtils.Bindings.Variables;

public class BindableVariableAlloc<T> : BindableVariableBase<T>
{
	public BindableVariableAlloc(T initialValue = default(T), bool checkEquality = true, Func<T, T, bool> equalityMethod = null, bool startInitialized = false)
		: base(initialValue, checkEquality, equalityMethod, startInitialized)
	{
	}
}
