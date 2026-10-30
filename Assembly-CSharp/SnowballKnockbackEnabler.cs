using UnityEngine;

[AddComponentMenu("GorillaTag/Seasonal/Snowball Knockback Enabler")]
public class SnowballKnockbackEnabler : MonoBehaviour
{
	private void OnEnable()
	{
		GrowingSnowballThrowable.NotifyEnableKnockbackIntent(this);
	}

	private void OnDisable()
	{
		GrowingSnowballThrowable.NotifyDisableKnockbackIntent(this);
	}
}
