using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.GrabAPI;

[Serializable]
public struct GrabbingRule
{
	[SerializeField]
	private FingerRequirement _thumbRequirement;

	[SerializeField]
	private FingerRequirement _indexRequirement;

	[SerializeField]
	private FingerRequirement _middleRequirement;

	[SerializeField]
	private FingerRequirement _ringRequirement;

	[SerializeField]
	private FingerRequirement _pinkyRequirement;

	[SerializeField]
	private FingerUnselectMode _unselectMode;

	public FingerUnselectMode UnselectMode => _unselectMode;

	public bool SelectsWithOptionals
	{
		get
		{
			if (_thumbRequirement != FingerRequirement.Required && _indexRequirement != FingerRequirement.Required && _middleRequirement != FingerRequirement.Required && _ringRequirement != FingerRequirement.Required)
			{
				return _pinkyRequirement != FingerRequirement.Required;
			}
			return false;
		}
	}

	public FingerRequirement this[HandFinger fingerID]
	{
		get
		{
			return fingerID switch
			{
				HandFinger.Thumb => _thumbRequirement, 
				HandFinger.Index => _indexRequirement, 
				HandFinger.Middle => _middleRequirement, 
				HandFinger.Ring => _ringRequirement, 
				HandFinger.Pinky => _pinkyRequirement, 
				_ => FingerRequirement.Ignored, 
			};
		}
		set
		{
			switch (fingerID)
			{
			case HandFinger.Thumb:
				_thumbRequirement = value;
				break;
			case HandFinger.Index:
				_indexRequirement = value;
				break;
			case HandFinger.Middle:
				_middleRequirement = value;
				break;
			case HandFinger.Ring:
				_ringRequirement = value;
				break;
			case HandFinger.Pinky:
				_pinkyRequirement = value;
				break;
			}
		}
	}

	public static GrabbingRule DefaultPalmRule { get; } = new GrabbingRule
	{
		_thumbRequirement = FingerRequirement.Optional,
		_indexRequirement = FingerRequirement.Required,
		_middleRequirement = FingerRequirement.Required,
		_ringRequirement = FingerRequirement.Required,
		_pinkyRequirement = FingerRequirement.Optional,
		_unselectMode = FingerUnselectMode.AllReleased
	};


	public static GrabbingRule DefaultPinchRule { get; } = new GrabbingRule
	{
		_thumbRequirement = FingerRequirement.Optional,
		_indexRequirement = FingerRequirement.Optional,
		_middleRequirement = FingerRequirement.Optional,
		_ringRequirement = FingerRequirement.Ignored,
		_pinkyRequirement = FingerRequirement.Ignored,
		_unselectMode = FingerUnselectMode.AllReleased
	};


	public static GrabbingRule FullGrab { get; } = new GrabbingRule
	{
		_thumbRequirement = FingerRequirement.Required,
		_indexRequirement = FingerRequirement.Required,
		_middleRequirement = FingerRequirement.Required,
		_ringRequirement = FingerRequirement.Required,
		_pinkyRequirement = FingerRequirement.Required,
		_unselectMode = FingerUnselectMode.AllReleased
	};


	public void StripIrrelevant(ref HandFingerFlags fingerFlags)
	{
		for (int i = 0; i < 5; i++)
		{
			HandFinger fingerID = (HandFinger)i;
			if (this[fingerID] == FingerRequirement.Ignored)
			{
				fingerFlags &= (HandFingerFlags)(~(1 << i));
			}
		}
	}

	public GrabbingRule(HandFingerFlags mask, in GrabbingRule otherRule)
	{
		_thumbRequirement = (((mask & HandFingerFlags.Thumb) != 0) ? otherRule._thumbRequirement : FingerRequirement.Ignored);
		_indexRequirement = (((mask & HandFingerFlags.Index) != 0) ? otherRule._indexRequirement : FingerRequirement.Ignored);
		_middleRequirement = (((mask & HandFingerFlags.Middle) != 0) ? otherRule._middleRequirement : FingerRequirement.Ignored);
		_ringRequirement = (((mask & HandFingerFlags.Ring) != 0) ? otherRule._ringRequirement : FingerRequirement.Ignored);
		_pinkyRequirement = (((mask & HandFingerFlags.Pinky) != 0) ? otherRule._pinkyRequirement : FingerRequirement.Ignored);
		_unselectMode = otherRule.UnselectMode;
	}
}
