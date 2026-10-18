namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class InteractableController : Controller
{
	private PointerHandler _handler;

	private bool _hover;

	protected bool Hover
	{
		get
		{
			return _hover;
		}
		private set
		{
			if (_hover != value)
			{
				_hover = value;
				OnHoverChanged();
			}
		}
	}

	protected override void Setup(Controller owner)
	{
		base.Setup(owner);
		_handler = base.GameObject.AddComponent<PointerHandler>();
		_handler.Controller = this;
	}

	public void OnPointerEnter()
	{
		Hover = true;
	}

	public void OnPointerExit()
	{
		Hover = false;
	}

	public virtual void OnPointerClick()
	{
	}

	protected virtual void OnHoverChanged()
	{
	}

	protected virtual void OnDisable()
	{
		Hover = false;
	}

	protected void PlayHaptics(OVRHapticsClip hapticsClip)
	{
		if (hapticsClip != null)
		{
			switch (OVRInput.GetActiveController())
			{
			case OVRInput.Controller.LTouch:
				OVRHaptics.LeftChannel.Mix(hapticsClip);
				break;
			case OVRInput.Controller.RTouch:
				OVRHaptics.RightChannel.Mix(hapticsClip);
				break;
			}
		}
	}
}
