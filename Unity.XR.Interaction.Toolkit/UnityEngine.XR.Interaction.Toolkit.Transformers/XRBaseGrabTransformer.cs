using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace UnityEngine.XR.Interaction.Toolkit.Transformers;

public abstract class XRBaseGrabTransformer : MonoBehaviour, IXRGrabTransformer
{
	public enum RegistrationMode
	{
		None,
		Single,
		Multiple,
		SingleAndMultiple
	}

	public virtual bool canProcess => base.isActiveAndEnabled;

	protected virtual RegistrationMode registrationMode => RegistrationMode.Single;

	internal RegistrationMode GetRegistrationMode()
	{
		return registrationMode;
	}

	protected virtual void Start()
	{
		if (!TryGetComponent<XRGrabInteractable>(out var component) || component.startingSingleGrabTransformers.Contains(this) || component.startingMultipleGrabTransformers.Contains(this))
		{
			return;
		}
		for (int num = component.singleGrabTransformersCount - 1; num >= 0; num--)
		{
			if (component.GetSingleGrabTransformerAt(num) == this)
			{
				return;
			}
		}
		for (int num2 = component.multipleGrabTransformersCount - 1; num2 >= 0; num2--)
		{
			if (component.GetMultipleGrabTransformerAt(num2) == this)
			{
				return;
			}
		}
		switch (registrationMode)
		{
		case RegistrationMode.Single:
			component.AddSingleGrabTransformer(this);
			break;
		case RegistrationMode.Multiple:
			component.AddMultipleGrabTransformer(this);
			break;
		case RegistrationMode.SingleAndMultiple:
			component.AddSingleGrabTransformer(this);
			component.AddMultipleGrabTransformer(this);
			break;
		case RegistrationMode.None:
			break;
		}
	}

	protected virtual void OnDestroy()
	{
		if (TryGetComponent<XRGrabInteractable>(out var component))
		{
			component.RemoveSingleGrabTransformer(this);
			component.RemoveMultipleGrabTransformer(this);
		}
	}

	public virtual void OnLink(XRGrabInteractable grabInteractable)
	{
	}

	public virtual void OnGrab(XRGrabInteractable grabInteractable)
	{
	}

	public virtual void OnGrabCountChanged(XRGrabInteractable grabInteractable, Pose targetPose, Vector3 localScale)
	{
	}

	public abstract void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale);

	public virtual void OnUnlink(XRGrabInteractable grabInteractable)
	{
	}
}
