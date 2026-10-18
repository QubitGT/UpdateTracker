using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Obsolete("Kept for migration purpose only. Do not use (see script for more info) #from(6000.0) (UnityUpgradable) -> FullScreenPassRendererFeature", true)]
internal class FullScreenPassRendererFeature_OldGUID : FullScreenPassRendererFeature, ISerializationCallbackReceiver
{
	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
	}
}
