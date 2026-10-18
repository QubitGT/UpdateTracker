using System;
using Meta.XR.Util;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(OVRSceneAnchor))]
[HelpURL("https://developer.oculus.com/documentation/unity/unity-scene-use-scene-anchors/#further-scene-model-unity-components")]
[Obsolete("OVRSceneManager and associated classes are deprecated (v65), please use MR Utility Kit instead (https://developer.oculus.com/documentation/unity/unity-mr-utility-kit-overview)")]
[Feature(Feature.Scene)]
public class OVRSceneVolume : MonoBehaviour, IOVRSceneComponent
{
	[Tooltip("When enabled, scales the child transforms according to the dimensions of this volume. If both Volume and Plane components exist on the game object, the volume takes precedence.")]
	[SerializeField]
	internal bool _scaleChildren = true;

	[Tooltip("When enabled, offsets the child transforms according to the offset of this volume. If both Volume and Plane components exist on the game object, the volume takes precedence.")]
	[SerializeField]
	internal bool _offsetChildren;

	private OVRSceneAnchor _sceneAnchor;

	public float Width { get; private set; }

	public float Height { get; private set; }

	public float Depth { get; private set; }

	public Vector3 Dimensions => new Vector3(Width, Height, Depth);

	public Vector3 Offset { get; private set; }

	public bool ScaleChildren
	{
		get
		{
			return _scaleChildren;
		}
		set
		{
			_scaleChildren = value;
			if (_scaleChildren && _sceneAnchor.Space.Valid)
			{
				SetChildScale();
			}
		}
	}

	public bool OffsetChildren
	{
		get
		{
			return _offsetChildren;
		}
		set
		{
			_offsetChildren = value;
			if (_offsetChildren && _sceneAnchor.Space.Valid)
			{
				SetChildOffset();
			}
		}
	}

	private void Awake()
	{
		_sceneAnchor = GetComponent<OVRSceneAnchor>();
		if (_sceneAnchor.Space.Valid)
		{
			((IOVRSceneComponent)this).Initialize();
		}
	}

	void IOVRSceneComponent.Initialize()
	{
		UpdateTransform();
	}

	private void SetChildScale()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			if (!child.TryGetComponent<OVRSceneObjectTransformType>(out var component) || component.TransformType == OVRSceneObjectTransformType.Transformation.Volume)
			{
				child.localScale = Dimensions;
			}
		}
	}

	private void SetChildOffset()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			if (!child.TryGetComponent<OVRSceneObjectTransformType>(out var component) || component.TransformType == OVRSceneObjectTransformType.Transformation.Volume)
			{
				child.localPosition = Offset;
			}
		}
	}

	internal void UpdateTransform()
	{
		if (OVRPlugin.GetSpaceBoundingBox3D(_sceneAnchor.Space, out var bounds))
		{
			Width = bounds.Size.w;
			Height = bounds.Size.h;
			Depth = bounds.Size.d;
			Vector3 position = base.transform.position;
			Vector3 vector = base.transform.TransformPoint(bounds.Pos.FromVector3f() + bounds.Size.FromSize3f());
			Vector3 vector2 = base.transform.TransformPoint(bounds.Pos.FromVector3f() + bounds.Size.FromSize3f() / 2f);
			vector2.y = vector.y;
			Offset = new Vector3(vector2.x - position.x, vector2.z - position.z, vector2.y - position.y);
			if (ScaleChildren)
			{
				SetChildScale();
			}
			if (OffsetChildren)
			{
				SetChildOffset();
			}
		}
	}
}
