using System;
using System.Collections.Generic;
using Oculus.Interaction.Body.Input;
using UnityEngine;

namespace Oculus.Interaction.Body.PoseDetection;

public class BodyPoseComparerActiveStateDebugVisual : MonoBehaviour
{
	[Tooltip("The PoseComparer to debug.")]
	[SerializeField]
	private BodyPoseComparerActiveState _bodyPoseComparer;

	[Tooltip("The body pose to overlay onto. This gizmo simply draws gizmos at joint locations - you must provide a body pose in order for this component to place the gizmos accurately.")]
	[SerializeField]
	[Interface(typeof(IBodyPose), new Type[] { })]
	private UnityEngine.Object _bodyPose;

	private IBodyPose BodyPose;

	[Tooltip("The root transform of the body on which to overlay the spheres. For BodyPoseDebugGizmos, this is simply the transform of the component. For a skinned body model, this would be the Root transform.")]
	[SerializeField]
	private Transform _root;

	[Tooltip("The radius of the debug spheres.")]
	[SerializeField]
	[Delayed]
	private float _radius = 0.1f;

	public float Radius
	{
		get
		{
			return _radius;
		}
		set
		{
			_radius = value;
		}
	}

	protected virtual void Awake()
	{
		BodyPose = _bodyPose as IBodyPose;
	}

	protected virtual void Start()
	{
	}

	protected virtual void Update()
	{
		DrawJointSpheres();
	}

	private void DrawJointSpheres()
	{
		foreach (KeyValuePair<BodyPoseComparerActiveState.JointComparerConfig, BodyPoseComparerActiveState.BodyPoseComparerFeatureState> featureState in _bodyPoseComparer.FeatureStates)
		{
			BodyJointId joint = featureState.Key.Joint;
			BodyPoseComparerActiveState.BodyPoseComparerFeatureState value = featureState.Value;
			if (BodyPose.GetJointPoseFromRoot(joint, out var pose))
			{
				Vector3 p = _root.TransformPoint(pose.position);
				Color color = ((value.Delta <= value.MaxDelta) ? Color.green : ((!(value.MaxDelta > 0f)) ? Color.red : Color.Lerp(t: value.Delta / value.MaxDelta / 2f, a: Color.yellow, b: Color.red)));
				DebugGizmos.LineWidth = _radius / 2f;
				DebugGizmos.Color = color;
				DebugGizmos.DrawPoint(p);
			}
		}
	}

	public void InjectAllBodyPoseComparerActiveStateDebugVisual(BodyPoseComparerActiveState bodyPoseComparer, IBodyPose bodyPose, Transform root)
	{
		InjectBodyPoseComparer(bodyPoseComparer);
		InjectBodyPose(bodyPose);
		InjectRootTransform(root);
	}

	public void InjectRootTransform(Transform root)
	{
		_root = root;
	}

	public void InjectBodyPoseComparer(BodyPoseComparerActiveState bodyPoseComparer)
	{
		_bodyPoseComparer = bodyPoseComparer;
	}

	public void InjectBodyPose(IBodyPose bodyPose)
	{
		_bodyPose = bodyPose as UnityEngine.Object;
		BodyPose = bodyPose;
	}
}
