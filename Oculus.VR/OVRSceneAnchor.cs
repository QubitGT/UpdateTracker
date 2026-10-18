using System;
using System.Collections.Generic;
using Meta.XR.Util;
using UnityEngine;

[DisallowMultipleComponent]
[HelpURL("https://developer.oculus.com/documentation/unity/unity-scene-use-scene-anchors/#ovrsceneanchor")]
[Obsolete("OVRSceneManager and associated classes are deprecated (v65), please use MR Utility Kit instead (https://developer.oculus.com/documentation/unity/unity-mr-utility-kit-overview)")]
[Feature(Feature.Scene)]
public sealed class OVRSceneAnchor : MonoBehaviour
{
	private static readonly Quaternion RotateY180 = Quaternion.Euler(0f, 180f, 0f);

	private OVRPlugin.Posef? _pose;

	private bool _isLocatable;

	private readonly List<OVRPlugin.SpaceComponentType> _supportedComponents = new List<OVRPlugin.SpaceComponentType>();

	private static readonly Dictionary<OVRSpace, int> AnchorReferenceCountDictionary = new Dictionary<OVRSpace, int>();

	internal static readonly Dictionary<Guid, OVRSceneAnchor> SceneAnchors = new Dictionary<Guid, OVRSceneAnchor>();

	internal static readonly List<OVRSceneAnchor> SceneAnchorsList = new List<OVRSceneAnchor>();

	public OVRSpace Space { get; private set; }

	public Guid Uuid { get; private set; }

	public OVRAnchor Anchor { get; private set; }

	public bool IsTracked { get; internal set; }

	private bool IsComponentSupported(OVRPlugin.SpaceComponentType spaceComponentType)
	{
		if (_supportedComponents.Count == 0 && !Anchor.GetSupportedComponents(_supportedComponents))
		{
			return false;
		}
		return _supportedComponents.Contains(spaceComponentType);
	}

	internal bool IsComponentEnabled(OVRPlugin.SpaceComponentType spaceComponentType)
	{
		bool flag = default(bool);
		bool changePending;
		return IsComponentSupported(spaceComponentType) && OVRPlugin.GetSpaceComponentStatus(Space, spaceComponentType, out flag, out changePending) && flag;
	}

	private void SyncComponent<T>(OVRPlugin.SpaceComponentType spaceComponentType) where T : MonoBehaviour, IOVRSceneComponent
	{
		if (IsComponentEnabled(spaceComponentType))
		{
			T component = GetComponent<T>();
			if ((bool)component)
			{
				component.Initialize();
			}
			else
			{
				base.gameObject.AddComponent<T>();
			}
		}
	}

	internal void ClearPoseCache()
	{
		_pose = null;
	}

	public void Initialize(OVRAnchor anchor)
	{
		OVRSpace space = anchor.Handle;
		Guid uuid = anchor.Uuid;
		if (Space.Valid)
		{
			throw new InvalidOperationException(string.Format("[{0}] {1} has already been initialized.", uuid, "OVRSceneAnchor"));
		}
		if (!space.Valid)
		{
			throw new ArgumentException(string.Format("[{0}] {1} must be valid.", uuid, "space"), "space");
		}
		Space = space;
		Uuid = uuid;
		Anchor = anchor;
		ClearPoseCache();
		SceneAnchors[Uuid] = this;
		SceneAnchorsList.Add(this);
		AnchorReferenceCountDictionary.TryGetValue(Space, out var value);
		AnchorReferenceCountDictionary[Space] = value + 1;
		_isLocatable = IsComponentSupported(OVRPlugin.SpaceComponentType.Locatable);
		if (base.enabled && _isLocatable)
		{
			TryUpdateTransform(useCache: false);
		}
		SyncComponent<OVRSemanticClassification>(OVRPlugin.SpaceComponentType.SemanticLabels);
		SyncComponent<OVRSceneVolume>(OVRPlugin.SpaceComponentType.Bounded3D);
		SyncComponent<OVRScenePlane>(OVRPlugin.SpaceComponentType.Bounded2D);
	}

	public void InitializeFrom(OVRSceneAnchor other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		Initialize(other.Anchor);
	}

	public static void GetSceneAnchors(List<OVRSceneAnchor> anchors)
	{
		if (anchors == null)
		{
			throw new ArgumentNullException("anchors");
		}
		anchors.Clear();
		anchors.AddRange(SceneAnchorsList);
	}

	public static void GetSceneAnchorsOfType<T>(List<T> anchors) where T : UnityEngine.Object
	{
		if (anchors == null)
		{
			throw new ArgumentNullException("anchors");
		}
		anchors.Clear();
		foreach (OVRSceneAnchor sceneAnchors in SceneAnchorsList)
		{
			if (sceneAnchors.TryGetComponent<T>(out var component))
			{
				anchors.Add(component);
			}
		}
	}

	internal bool TryUpdateTransform(bool useCache)
	{
		if (!Space.Valid || !base.enabled || !_isLocatable)
		{
			return false;
		}
		if (!useCache || !_pose.HasValue)
		{
			if (!(IsTracked = OVRPlugin.TryLocateSpace(Space, OVRPlugin.GetTrackingOriginType(), out var pose, out var locationFlags) && locationFlags.IsOrientationValid() && locationFlags.IsPositionValid()))
			{
				return false;
			}
			_pose = pose;
		}
		OVRPose trackingSpacePose = default(OVRPose);
		trackingSpacePose.position = _pose.Value.Position.FromFlippedZVector3f();
		trackingSpacePose.orientation = _pose.Value.Orientation.FromFlippedZQuatf() * RotateY180;
		OVRPose oVRPose = trackingSpacePose.ToWorldSpacePose(Camera.main);
		base.transform.SetPositionAndRotation(oVRPose.position, oVRPose.orientation);
		return true;
	}

	private void OnDestroy()
	{
		SceneAnchors.Remove(Uuid);
		SceneAnchorsList.Remove(this);
		if (!Space.Valid || !AnchorReferenceCountDictionary.TryGetValue(Space, out var value))
		{
			return;
		}
		if (value == 1)
		{
			if (Space.Valid)
			{
				OVRPlugin.DestroySpace(Space);
			}
			AnchorReferenceCountDictionary.Remove(Space);
		}
		else
		{
			AnchorReferenceCountDictionary[Space] = value - 1;
		}
	}
}
