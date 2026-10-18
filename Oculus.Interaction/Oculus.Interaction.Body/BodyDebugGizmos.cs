using System;
using Oculus.Interaction.Body.Input;
using UnityEngine;

namespace Oculus.Interaction.Body;

public class BodyDebugGizmos : SkeletonDebugGizmos
{
	public enum CoordSpace
	{
		World,
		Local
	}

	[SerializeField]
	[Interface(typeof(IBody), new Type[] { })]
	private UnityEngine.Object _body;

	private IBody Body;

	[Tooltip("The coordinate space in which to draw the skeleton. World space draws the skeleton at the world Body location. Local draws the skeleton relative to this transform's position, and can be placed, scaled, or mirrored as desired.")]
	[SerializeField]
	private CoordSpace _space;

	protected bool _started;

	public CoordSpace Space
	{
		get
		{
			return _space;
		}
		set
		{
			_space = value;
		}
	}

	protected virtual void Awake()
	{
		Body = _body as IBody;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			Body.WhenBodyUpdated += HandleBodyUpdated;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Body.WhenBodyUpdated -= HandleBodyUpdated;
		}
	}

	protected override bool TryGetJointPose(int jointId, out Pose pose)
	{
		CoordSpace space = _space;
		bool result;
		if (space == CoordSpace.World || space != CoordSpace.Local)
		{
			result = Body.GetJointPose((BodyJointId)jointId, out pose);
		}
		else
		{
			result = Body.GetJointPoseFromRoot((BodyJointId)jointId, out pose);
			pose.position = base.transform.TransformPoint(pose.position);
			pose.rotation = base.transform.rotation * pose.rotation;
		}
		return result;
	}

	protected override bool TryGetParentJointId(int jointId, out int parent)
	{
		if (Body.SkeletonMapping.TryGetParentJointId((BodyJointId)jointId, out var parent2))
		{
			parent = (int)parent2;
			return true;
		}
		parent = 0;
		return false;
	}

	private VisibilityFlags GetModifiedDrawFlags()
	{
		VisibilityFlags visibilityFlags = base.Visibility;
		if (base.HasNegativeScale && Space == CoordSpace.Local)
		{
			visibilityFlags &= ~VisibilityFlags.Axes;
		}
		return visibilityFlags;
	}

	private void HandleBodyUpdated()
	{
		foreach (BodyJointId joint in Body.SkeletonMapping.Joints)
		{
			Draw((int)joint, GetModifiedDrawFlags());
		}
	}

	public void InjectAllBodyJointDebugGizmos(IBody body)
	{
		InjectBody(body);
	}

	public void InjectBody(IBody body)
	{
		_body = body as UnityEngine.Object;
		Body = body;
	}
}
