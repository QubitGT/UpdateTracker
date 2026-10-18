using System;
using System.Collections.Generic;
using Meta.XR.Acoustics;
using UnityEngine;

internal sealed class MetaXRAcousticControlZone : MonoBehaviour
{
	[Serializable]
	internal class State
	{
		[SerializeField]
		internal Color color = Color.blue;

		[SerializeField]
		internal Spectrum rt60 = new Spectrum();

		[SerializeField]
		internal Spectrum reverbLevel = new Spectrum();

		[SerializeField]
		internal float fadeDistance = 1f;

		internal void Clone(State other)
		{
			color = other.color;
			reverbLevel.Clone(other.reverbLevel);
			rt60.Clone(other.rt60);
			fadeDistance = other.fadeDistance;
		}
	}

	[SerializeField]
	private State _state = new State();

	private IntPtr _controlHandle = IntPtr.Zero;

	internal State state => _state;

	internal Color ZoneColor
	{
		get
		{
			return _state.color;
		}
		set
		{
			_state.color = value;
		}
	}

	internal Spectrum Rt60
	{
		get
		{
			return _state.rt60;
		}
		set
		{
			_state.rt60 = value;
		}
	}

	internal Spectrum ReverbLevel
	{
		get
		{
			return _state.reverbLevel;
		}
		set
		{
			_state.reverbLevel = value;
		}
	}

	internal float FadeDistance
	{
		get
		{
			return _state.fadeDistance;
		}
		set
		{
			_state.fadeDistance = value;
			ApplyTransform();
		}
	}

	private Vector3 NativeFadeDistance => new Vector3(_state.fadeDistance / base.transform.localScale.x, _state.fadeDistance / base.transform.localScale.y, _state.fadeDistance / base.transform.localScale.z);

	private Vector3 NativeBoxSize => new Vector3(2f + NativeFadeDistance.x, 2f + NativeFadeDistance.y, 2f + NativeFadeDistance.z);

	internal void Clone(State other)
	{
		_state.Clone(other);
	}

	internal MetaXRAcousticControlZone()
	{
		Rt60.points = new List<Spectrum.Point>
		{
			new Spectrum.Point(1000f)
		};
		ReverbLevel.points = new List<Spectrum.Point>
		{
			new Spectrum.Point(1000f)
		};
	}

	private void Start()
	{
		StartInternal();
	}

	internal void StartInternal()
	{
		if (!(_controlHandle != IntPtr.Zero))
		{
			if (MetaXRAcousticNativeInterface.Interface.CreateControlZone(out _controlHandle) != 0)
			{
				Debug.LogError("Unable to create internal Control Zone", base.gameObject);
			}
			else
			{
				ApplyProperties();
			}
		}
	}

	private void OnDestroy()
	{
		DestroyInternal();
	}

	internal void DestroyInternal()
	{
		if (_controlHandle != IntPtr.Zero)
		{
			MetaXRAcousticNativeInterface.Interface.DestroyControlZone(_controlHandle);
			_controlHandle = IntPtr.Zero;
		}
	}

	private void OnEnable()
	{
		if (!(_controlHandle == IntPtr.Zero))
		{
			MetaXRAcousticNativeInterface.Interface.ControlZoneSetEnabled(_controlHandle, enabled: true);
		}
	}

	private void OnDisable()
	{
		if (!(_controlHandle == IntPtr.Zero))
		{
			MetaXRAcousticNativeInterface.Interface.ControlZoneSetEnabled(_controlHandle, enabled: false);
		}
	}

	private void LateUpdate()
	{
		if (!(_controlHandle == IntPtr.Zero) && base.transform.hasChanged)
		{
			ApplyTransform();
			base.transform.hasChanged = false;
		}
	}

	private void ApplyTransform()
	{
		if (!(_controlHandle == IntPtr.Zero))
		{
			MetaXRAcousticNativeInterface.Interface.ControlZoneSetBox(_controlHandle, NativeBoxSize.x, NativeBoxSize.y, NativeBoxSize.z);
			MetaXRAcousticNativeInterface.Interface.ControlZoneSetFadeDistance(_controlHandle, NativeFadeDistance.x, NativeFadeDistance.y, NativeFadeDistance.z);
			MetaXRAcousticNativeInterface.INativeInterface @interface = MetaXRAcousticNativeInterface.Interface;
			IntPtr controlHandle = _controlHandle;
			Matrix4x4 matrix = base.transform.localToWorldMatrix;
			@interface.ControlZoneSetTransform(controlHandle, in matrix);
		}
	}

	internal void ApplyProperties()
	{
		if (_controlHandle == IntPtr.Zero)
		{
			return;
		}
		ApplyTransform();
		MetaXRAcousticNativeInterface.Interface.ControlZoneReset(_controlHandle, ControlZoneProperty.RT60);
		foreach (Spectrum.Point point in Rt60.points)
		{
			MetaXRAcousticNativeInterface.Interface.ControlZoneSetFrequency(_controlHandle, ControlZoneProperty.RT60, point.frequency, point.data);
		}
		MetaXRAcousticNativeInterface.Interface.ControlZoneReset(_controlHandle, ControlZoneProperty.REVERB_LEVEL);
		foreach (Spectrum.Point point2 in ReverbLevel.points)
		{
			MetaXRAcousticNativeInterface.Interface.ControlZoneSetFrequency(_controlHandle, ControlZoneProperty.REVERB_LEVEL, point2.frequency, point2.data);
		}
	}
}
