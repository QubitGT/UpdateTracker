using System;

namespace Unity.XR.CoreUtils.Bindings.Variables;

public class BindableVariable<T> : BindableVariableBase<T> where T : IEquatable<T>
{
	public BindableVariable(T initialValue = default(T), bool checkEquality = true, Func<T, T, bool> equalityMethod = null, bool startInitialized = false)
		: base(initialValue, checkEquality, equalityMethod, startInitialized)
	{
	}

	public override bool ValueEquals(T other)
	{
		return base.Value.Equals(other);
	}
}
