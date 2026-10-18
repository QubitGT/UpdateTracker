using System;
using Meta.XR.Util;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
[HelpURL("https://developer.oculus.com/documentation/unity/move-face-tracking/")]
[Feature(Feature.FaceTracking)]
public class OVRCustomFace : OVRFace
{
	public enum RetargetingType
	{
		OculusFace,
		Custom
	}

	[SerializeField]
	[Tooltip("The mapping between Face Expressions to the blend shapes available on the shared mesh of the skinned mesh renderer")]
	internal OVRFaceExpressions.FaceExpression[] _mappings;

	[SerializeField]
	[HideInInspector]
	internal RetargetingType retargetingType;

	[SerializeField]
	[Tooltip("Allow duplicates when mapping blend shapes to Face Expressions")]
	internal bool _allowDuplicateMapping = true;

	public OVRFaceExpressions.FaceExpression[] Mappings
	{
		get
		{
			return _mappings;
		}
		set
		{
			_mappings = value;
		}
	}

	protected RetargetingType RetargetingValue
	{
		get
		{
			return retargetingType;
		}
		set
		{
			retargetingType = value;
		}
	}

	protected bool AllowDuplicateMapping
	{
		get
		{
			return _allowDuplicateMapping;
		}
		set
		{
			_allowDuplicateMapping = value;
		}
	}

	protected override void Start()
	{
		base.Start();
	}

	protected internal override OVRFaceExpressions.FaceExpression GetFaceExpression(int blendShapeIndex)
	{
		return _mappings[blendShapeIndex];
	}

	protected internal virtual (string[], OVRFaceExpressions.FaceExpression[]) GetCustomBlendShapeNameAndExpressionPairs()
	{
		string[] names = Enum.GetNames(typeof(OVRFaceExpressions.FaceExpression));
		OVRFaceExpressions.FaceExpression[] item = (OVRFaceExpressions.FaceExpression[])Enum.GetValues(typeof(OVRFaceExpressions.FaceExpression));
		return (names, item);
	}
}
