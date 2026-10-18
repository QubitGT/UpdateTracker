using System;

namespace Fusion;

[Serializable]
public abstract class FieldsMask
{
	public Mask256 Mask;

	protected FieldsMask(Mask256 mask)
	{
		Mask = mask;
	}

	protected FieldsMask(long a, long b, long c, long d)
	{
		Mask = default(Mask256);
	}

	protected FieldsMask()
	{
	}

	public static implicit operator Mask256(FieldsMask mask)
	{
		return mask.Mask;
	}
}
[Serializable]
public class FieldsMask<T> : FieldsMask
{
	public FieldsMask(Mask256 mask)
		: base(mask)
	{
	}

	public FieldsMask(long maskA, long maskB = 0L, long maskC = 0L, long maskD = 0L)
		: base(maskA, maskB, maskC, maskD)
	{
	}

	public FieldsMask()
	{
	}

	public FieldsMask(Func<Mask256> getDefaultsDelegate)
	{
		Mask = getDefaultsDelegate?.Invoke() ?? default(Mask256);
	}
}
