using System;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class Button : InteractableController
{
	private static OVRHapticsClip _hapticsClip;

	private static OVRHapticsClip HapticsClip
	{
		get
		{
			if (OVRHaptics.Config.SampleSizeInBytes == 0)
			{
				return null;
			}
			return _hapticsClip ?? (_hapticsClip = new OVRHapticsClip(new byte[5] { 128, 255, 255, 128, 255 }, 5));
		}
	}

	public Action Callback { get; set; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init()
	{
		_hapticsClip = null;
	}

	public override void OnPointerClick()
	{
		Callback?.Invoke();
		Telemetry.OnButtonClicked(this);
	}

	protected override void OnHoverChanged()
	{
		base.OnHoverChanged();
		if (base.Hover)
		{
			PlayHaptics(HapticsClip);
		}
	}
}
