using System;
using System.Collections.Generic;
using Oculus.Interaction.Surfaces;
using UnityEngine;

namespace Oculus.Interaction;

public class SurfaceSnapPoseDelegate : MonoBehaviour, ISnapPoseDelegate
{
	[SerializeField]
	[Interface(typeof(ISurface), new Type[] { })]
	private UnityEngine.Object _surface;

	protected ISurface Surface;

	private Dictionary<int, Pose> _snappedPoses;

	protected virtual void Awake()
	{
		Surface = _surface as ISurface;
		_snappedPoses = new Dictionary<int, Pose>();
	}

	protected virtual void Start()
	{
	}

	public void TrackElement(int id, Pose p)
	{
	}

	public void UntrackElement(int id)
	{
	}

	private bool ComputeWorldSurfacePose(Pose pose, out Pose result)
	{
		if (Surface.ClosestSurfacePoint(in pose.position, out var hit))
		{
			result = new Pose(hit.Point, Quaternion.LookRotation(hit.Normal, pose.up));
			return true;
		}
		result = pose;
		return false;
	}

	private bool ComputeLocalSurfacePose(Pose pose, out Pose result)
	{
		if (ComputeWorldSurfacePose(pose, out var result2))
		{
			result = new Pose(Surface.Transform.InverseTransformPoint(result2.position), Quaternion.Inverse(result2.rotation) * Surface.Transform.rotation);
			return true;
		}
		result = pose;
		return false;
	}

	public void SnapElement(int id, Pose pose)
	{
		if (ComputeLocalSurfacePose(pose, out var result))
		{
			_snappedPoses.Add(id, result);
		}
		else
		{
			_snappedPoses.Add(id, pose);
		}
	}

	public void UnsnapElement(int id)
	{
		_snappedPoses.Remove(id);
	}

	public void MoveTrackedElement(int id, Pose p)
	{
	}

	public bool SnapPoseForElement(int id, Pose pose, out Pose result)
	{
		if (_snappedPoses.TryGetValue(id, out var value))
		{
			result = new Pose(Surface.Transform.TransformPoint(value.position), Surface.Transform.rotation * value.rotation);
			return true;
		}
		return ComputeWorldSurfacePose(pose, out result);
	}

	public void InjectAllSurfaceSnapPoseDelegate(ISurface surface)
	{
		InjectSurface(surface);
	}

	public void InjectSurface(ISurface surface)
	{
		_surface = surface as UnityEngine.Object;
		Surface = surface;
	}
}
