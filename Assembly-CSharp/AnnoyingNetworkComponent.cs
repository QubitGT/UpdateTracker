using System;

public abstract class AnnoyingNetworkComponent : NetworkComponent, IAnnoyingFeature
{
	public static event Action<IAnnoyingFeature, bool> AlertActive;

	internal override void OnEnable()
	{
		base.OnEnable();
		AnnoyingNetworkComponent.AlertActive?.Invoke(this, arg2: true);
	}

	internal override void OnDisable()
	{
		base.OnDisable();
		AnnoyingNetworkComponent.AlertActive?.Invoke(this, arg2: false);
	}

	public abstract void Allowed(bool v);
}
