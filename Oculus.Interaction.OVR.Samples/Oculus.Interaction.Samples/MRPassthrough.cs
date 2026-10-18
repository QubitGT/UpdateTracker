using System;
using UnityEngine;
using UnityEngine.UI;

namespace Oculus.Interaction.Samples;

public class MRPassthrough : MonoBehaviour
{
	public static class PassThrough
	{
		public static bool IsPassThroughOn;

		public static bool IsPassThroughCompatible;
	}

	[Tooltip("Objects that shouldn't be rendered during passthrough")]
	[Header("Passthrough Objects To Remove")]
	[SerializeField]
	private GameObject[] _objects;

	[Tooltip("These are UI objects that should be toggled ON/OFF during passthrough button")]
	[SerializeField]
	private Toggle _passThroughToggle;

	[Tooltip("The OVRPassthrough Layer")]
	[SerializeField]
	private OVRPassthroughLayer _layer;

	[Tooltip("Use the CenterEyeAnchor or Center Camera")]
	[SerializeField]
	private Camera _camera;

	protected bool _started;

	protected virtual void Reset()
	{
		_layer = UnityEngine.Object.FindFirstObjectByType<OVRPassthroughLayer>();
		_camera = OVRManager.FindMainCamera();
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
			ValidatePassthrough();
		}
	}

	private void ValidatePassthrough()
	{
		if (OVRManager.HasInsightPassthroughInitFailed())
		{
			_camera.clearFlags = CameraClearFlags.Skybox;
			_passThroughToggle.enabled = false;
			return;
		}
		if (PassThrough.IsPassThroughOn)
		{
			TurnPassThroughOn();
			return;
		}
		TurnPassThroughOff();
		if (PassThrough.IsPassThroughCompatible)
		{
			_passThroughToggle.enabled = false;
		}
		else
		{
			_passThroughToggle.enabled = true;
		}
	}

	[Obsolete]
	public void TurnLocoMotionSceneOn()
	{
		PassThrough.IsPassThroughCompatible = true;
	}

	[Obsolete]
	public void TurnLocoMotionSceneOff()
	{
		PassThrough.IsPassThroughCompatible = false;
	}

	public void TogglePassThrough()
	{
		if (PassThrough.IsPassThroughOn)
		{
			TurnPassThroughOff();
		}
		else
		{
			TurnPassThroughOn();
		}
	}

	public void CheckPassthroughToggle()
	{
		if (_passThroughToggle.enabled && PassThrough.IsPassThroughOn)
		{
			_passThroughToggle.isOn = true;
			TurnPassThroughOn();
		}
	}

	private void TurnPassThroughOn()
	{
		if (OVRManager.IsInsightPassthroughInitialized())
		{
			PassThrough.IsPassThroughOn = true;
			_layer.textureOpacity = 1f;
			_camera.clearFlags = CameraClearFlags.Color;
			GameObject[] objects = _objects;
			for (int i = 0; i < objects.Length; i++)
			{
				objects[i].SetActive(value: false);
			}
		}
		else
		{
			Debug.LogError("Failed to initialize Passthrough please check the OVRManager in the Hierarchy and check if Passthrough is supported and enabled.");
		}
	}

	private void TurnPassThroughOff()
	{
		PassThrough.IsPassThroughOn = false;
		_layer.textureOpacity = 0f;
		_camera.clearFlags = CameraClearFlags.Skybox;
		GameObject[] objects = _objects;
		for (int i = 0; i < objects.Length; i++)
		{
			objects[i].SetActive(value: true);
		}
	}

	public void InjectAllMRPassthrough(GameObject[] objects, Toggle passThroughToggle, OVRPassthroughLayer layer, Camera camera)
	{
		InjectObjects(objects);
		InjectPassThroughToggle(passThroughToggle);
		InjectLayer(layer);
		InjectCamera(camera);
	}

	public void InjectObjects(GameObject[] objects)
	{
		_objects = objects;
	}

	public void InjectPassThroughToggle(Toggle passThroughToggle)
	{
		_passThroughToggle = passThroughToggle;
	}

	public void InjectLayer(OVRPassthroughLayer layer)
	{
		_layer = layer;
	}

	public void InjectCamera(Camera camera)
	{
		_camera = camera;
	}
}
