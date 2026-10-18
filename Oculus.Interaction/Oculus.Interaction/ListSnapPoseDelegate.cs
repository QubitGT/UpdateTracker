using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction;

public class ListSnapPoseDelegate : MonoBehaviour, ISnapPoseDelegate
{
	private HashSet<int> _snappedIds;

	private ListLayout _layout;

	private ListLayoutEase _layoutEase;

	[SerializeField]
	private float _defaultSize = 1f;

	public float Size => _layout.Size;

	protected virtual void Start()
	{
		_snappedIds = new HashSet<int>();
		_layout = new ListLayout();
		_layoutEase = new ListLayoutEase(_layout);
		_layoutEase.UpdateTime(Time.timeSinceLevelLoad);
	}

	protected virtual void Update()
	{
		_layoutEase.UpdateTime(Time.timeSinceLevelLoad);
	}

	protected virtual float SizeForId(int id)
	{
		return _defaultSize;
	}

	protected virtual float FloatForPose(Pose pose)
	{
		return base.transform.InverseTransformPoint(pose.position).x;
	}

	protected virtual Pose PoseForFloat(float position)
	{
		return new Pose(base.transform.TransformPoint(new Vector3(position, 0f, 0f)), base.transform.rotation);
	}

	public void TrackElement(int id, Pose p)
	{
		_layout.AddElement(id, SizeForId(id), FloatForPose(p));
	}

	public void UntrackElement(int id)
	{
		_layout.RemoveElement(id);
	}

	public void SnapElement(int id, Pose pose)
	{
		_snappedIds.Add(id);
	}

	public void UnsnapElement(int id)
	{
		_snappedIds.Remove(id);
	}

	public void MoveTrackedElement(int id, Pose p)
	{
		_layout.MoveElement(id, FloatForPose(p));
	}

	public bool SnapPoseForElement(int id, Pose pose, out Pose result)
	{
		if (_snappedIds.Contains(id))
		{
			result = PoseForFloat(_layoutEase.GetPosition(id));
		}
		else
		{
			result = PoseForFloat(_layout.GetTargetPosition(id, FloatForPose(pose), SizeForId(id)));
		}
		return true;
	}
}
