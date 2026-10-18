using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

public class HmdRef : MonoBehaviour, IHmd
{
	[SerializeField]
	[Interface(typeof(IHmd), new Type[] { })]
	private UnityEngine.Object _hmd;

	private IHmd Hmd;

	public event Action WhenUpdated
	{
		add
		{
			Hmd.WhenUpdated += value;
		}
		remove
		{
			Hmd.WhenUpdated -= value;
		}
	}

	protected virtual void Awake()
	{
		Hmd = _hmd as IHmd;
	}

	protected virtual void Start()
	{
	}

	public bool TryGetRootPose(out Pose pose)
	{
		return Hmd.TryGetRootPose(out pose);
	}

	public void InjectAllHmdRef(IHmd hmd)
	{
		InjectHmd(hmd);
	}

	public void InjectHmd(IHmd hmd)
	{
		_hmd = hmd as UnityEngine.Object;
		Hmd = hmd;
	}
}
