using System;
using UnityEngine;

namespace Meta.WitAi.Utilities;

[Serializable]
public struct VoiceServiceReference
{
	[SerializeField]
	internal VoiceService voiceService;

	public VoiceService VoiceService
	{
		get
		{
			if (!voiceService)
			{
				VoiceService[] array = Resources.FindObjectsOfTypeAll<VoiceService>();
				if (array != null)
				{
					voiceService = Array.Find(array, (VoiceService o) => o.gameObject.scene.rootCount != 0);
				}
			}
			return voiceService;
		}
	}
}
