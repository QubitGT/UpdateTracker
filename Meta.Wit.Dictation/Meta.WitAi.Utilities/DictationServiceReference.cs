using System;
using Meta.WitAi.Dictation;
using UnityEngine;

namespace Meta.WitAi.Utilities;

[Serializable]
public struct DictationServiceReference
{
	[SerializeField]
	internal DictationService dictationService;

	public DictationService DictationService
	{
		get
		{
			if (!dictationService)
			{
				DictationService[] array = Resources.FindObjectsOfTypeAll<DictationService>();
				if (array != null)
				{
					dictationService = Array.Find(array, (DictationService o) => o.gameObject.scene.rootCount != 0);
				}
			}
			return dictationService;
		}
	}
}
