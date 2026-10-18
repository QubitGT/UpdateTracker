using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

public class HmdOffset : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IHmd), new Type[] { })]
	private UnityEngine.Object _hmd;

	private IHmd Hmd;

	[SerializeField]
	private Vector3 _offsetTranslation = Vector3.zero;

	[SerializeField]
	private Vector3 _offsetRotation = Vector3.zero;

	[SerializeField]
	private bool _disablePitchFromSource;

	[SerializeField]
	private bool _disableYawFromSource;

	[SerializeField]
	private bool _disableRollFromSource;

	protected bool _started;

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
			Hmd.WhenUpdated += HandleHmdUpdated;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Hmd.WhenUpdated -= HandleHmdUpdated;
		}
	}

	protected virtual void HandleHmdUpdated()
	{
		if (Hmd.TryGetRootPose(out var pose))
		{
			Vector3 position = pose.position;
			Quaternion rotation = pose.rotation;
			Vector3 eulerAngles = rotation.eulerAngles;
			Quaternion quaternion = Quaternion.Euler(new Vector3(eulerAngles.x, 0f, 0f));
			Quaternion quaternion2 = Quaternion.Euler(new Vector3(0f, eulerAngles.y, 0f));
			Quaternion quaternion3 = Quaternion.Euler(new Vector3(0f, 0f, eulerAngles.z));
			Quaternion identity = Quaternion.identity;
			if (!_disableYawFromSource)
			{
				identity *= quaternion2;
			}
			if (!_disablePitchFromSource)
			{
				identity *= quaternion;
			}
			if (!_disableRollFromSource)
			{
				identity *= quaternion3;
			}
			Quaternion quaternion4 = identity * Quaternion.Euler(_offsetRotation);
			base.transform.position = position + quaternion4 * _offsetTranslation;
			base.transform.rotation = quaternion4;
		}
	}

	public void InjectAllHmdOffset(IHmd hmd)
	{
		InjectHmd(hmd);
	}

	public void InjectHmd(IHmd hmd)
	{
		_hmd = hmd as UnityEngine.Object;
		Hmd = hmd;
	}

	public void InjectOptionalOffsetTranslation(Vector3 val)
	{
		_offsetTranslation = val;
	}

	public void InjectOptionalOffsetRotation(Vector3 val)
	{
		_offsetRotation = val;
	}

	public void InjectOptionalDisablePitchFromSource(bool val)
	{
		_disablePitchFromSource = val;
	}

	public void InjectOptionalDisableYawFromSource(bool val)
	{
		_disableYawFromSource = val;
	}

	public void InjectOptionalDisableRollFromSource(bool val)
	{
		_disableRollFromSource = val;
	}
}
