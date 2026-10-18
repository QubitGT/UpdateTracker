using System.Collections.Generic;

namespace UnityEngine.VFX.Utility;

[RequireComponent(typeof(Collider))]
internal class VFXTriggerEventBinder : VFXEventBinderBase
{
	public enum Activation
	{
		OnEnter,
		OnExit,
		OnStay
	}

	public List<Collider> colliders = new List<Collider>();

	public Activation activation;

	private ExposedProperty positionParameter = "position";

	protected override void SetEventAttribute(object[] parameters)
	{
		Collider collider = (Collider)parameters[0];
		eventAttribute.SetVector3(positionParameter, collider.transform.position);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (activation == Activation.OnEnter && colliders.Contains(other))
		{
			SendEventToVisualEffect(other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (activation == Activation.OnExit && colliders.Contains(other))
		{
			SendEventToVisualEffect(other);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (activation == Activation.OnStay && colliders.Contains(other))
		{
			SendEventToVisualEffect(other);
		}
	}
}
