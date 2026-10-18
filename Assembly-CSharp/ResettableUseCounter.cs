using System;

public struct ResettableUseCounter
{
	private int usesRemaining;

	private int maxRegularUses;

	private int maxSuperchargeUses;

	private Action<bool> onReadyChanged;

	public bool IsReady => usesRemaining > 0;

	public ResettableUseCounter(int maxRegularUses, int maxSuperchargeUses, Action<bool> onReadyChanged = null)
	{
		this.maxRegularUses = maxRegularUses;
		this.maxSuperchargeUses = maxSuperchargeUses;
		usesRemaining = maxRegularUses;
		this.onReadyChanged = onReadyChanged;
	}

	public bool TryUse()
	{
		if (!IsReady)
		{
			return false;
		}
		bool flag = SuperInfectionManager.activeSuperInfectionManager?.IsSupercharged ?? false;
		if (usesRemaining > maxRegularUses && !flag)
		{
			usesRemaining = maxRegularUses;
		}
		usesRemaining--;
		if (!IsReady)
		{
			onReadyChanged?.Invoke(obj: false);
		}
		return true;
	}

	public void Reset()
	{
		bool isReady = IsReady;
		usesRemaining = maxSuperchargeUses;
		if (!isReady)
		{
			onReadyChanged?.Invoke(obj: true);
		}
	}
}
