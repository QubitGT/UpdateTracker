using System;
using UnityEngine;

public class MedalClipDispatcher : MonoBehaviour
{
	public static event Action<MedalClipRelay.MedalClip.ClipType> DispatchClipEvent;

	public void DispatchClip(MedalClipRelay.MedalClip.ClipType v)
	{
		MedalClipDispatcher.DispatchClipEvent?.Invoke(v);
	}
}
