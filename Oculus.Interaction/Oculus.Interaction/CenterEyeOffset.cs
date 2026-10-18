using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class CenterEyeOffset : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IHmd), new Type[] { })]
	private UnityEngine.Object _hmd;

	[SerializeField]
	private Vector3 _offset;

	[SerializeField]
	private Quaternion _rotation = Quaternion.identity;

	private Pose _cachedPose = Pose.identity;

	protected bool _started;

	public IHmd Hmd { get; private set; }

	protected virtual void Awake()
	{
		Hmd = _hmd as IHmd;
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
			Hmd.WhenUpdated += HandleUpdated;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Hmd.WhenUpdated -= HandleUpdated;
		}
	}

	private void HandleUpdated()
	{
		if (Hmd.TryGetRootPose(out var pose))
		{
			GetOffset(ref _cachedPose);
			_cachedPose.Postmultiply(in pose);
			base.transform.SetPose(in _cachedPose);
		}
	}

	public void GetOffset(ref Pose pose)
	{
		pose.position = _offset;
		pose.rotation = _rotation;
	}

	public void GetWorldPose(ref Pose pose)
	{
		pose.position = base.transform.position;
		pose.rotation = base.transform.rotation;
	}

	public void InjectOffset(Vector3 offset)
	{
		_offset = offset;
	}

	public void InjectRotation(Quaternion rotation)
	{
		_rotation = rotation;
	}

	public void InjectAllCenterEyeOffset(IHmd hmd, Vector3 offset, Quaternion rotation)
	{
		InjectHmd(hmd);
		InjectOffset(offset);
		InjectRotation(rotation);
	}

	public void InjectHmd(IHmd hmd)
	{
		Hmd = hmd;
		_hmd = hmd as UnityEngine.Object;
	}
}
