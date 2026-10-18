using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.VFX.Utility;

[Serializable]
internal class VisualEffectActivationBehaviour : PlayableBehaviour
{
	[Serializable]
	public enum AttributeType
	{
		Float = 1,
		Float2 = 2,
		Float3 = 3,
		Float4 = 4,
		Int32 = 5,
		Uint32 = 6,
		Boolean = 17
	}

	[Serializable]
	public struct EventState
	{
		public ExposedProperty attribute;

		public AttributeType type;

		public float[] values;
	}

	[SerializeField]
	public ExposedProperty onClipEnter = "OnPlay";

	[SerializeField]
	public ExposedProperty onClipExit = "OnStop";

	[SerializeField]
	public EventState[] clipEnterEventAttributes;

	[SerializeField]
	public EventState[] clipExitEventAttributes;
}
