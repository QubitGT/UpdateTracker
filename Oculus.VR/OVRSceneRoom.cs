using System;
using System.Collections.Generic;
using System.Diagnostics;
using Meta.XR.Util;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(OVRSceneAnchor))]
[HelpURL("https://developer.oculus.com/documentation/unity/unity-scene-use-scene-anchors/#further-scene-model-unity-components")]
[Obsolete("OVRSceneManager and associated classes are deprecated (v65), please use MR Utility Kit instead (https://developer.oculus.com/documentation/unity/unity-mr-utility-kit-overview)")]
[Feature(Feature.Scene)]
public class OVRSceneRoom : MonoBehaviour, IOVRSceneComponent
{
	private OVRSceneAnchor _sceneAnchor;

	private OVRSceneManager _sceneManager;

	private Guid _uuid;

	internal static readonly Dictionary<Guid, OVRSceneRoom> SceneRooms = new Dictionary<Guid, OVRSceneRoom>();

	internal static readonly List<OVRSceneRoom> SceneRoomsList = new List<OVRSceneRoom>();

	public OVRScenePlane Floor { get; private set; }

	public OVRScenePlane Ceiling { get; private set; }

	public OVRScenePlane[] Walls { get; private set; } = Array.Empty<OVRScenePlane>();


	private void Awake()
	{
		_sceneAnchor = GetComponent<OVRSceneAnchor>();
		_sceneManager = UnityEngine.Object.FindAnyObjectByType<OVRSceneManager>();
		_uuid = _sceneAnchor.Uuid;
		if (_sceneAnchor.Space.Valid)
		{
			((IOVRSceneComponent)this).Initialize();
		}
	}

	void IOVRSceneComponent.Initialize()
	{
		SceneRooms[_uuid] = this;
		SceneRoomsList.Add(this);
	}

	internal async OVRTask<bool> LoadRoom(Guid floor, Guid ceiling, Guid[] walls)
	{
		HashSet<Guid> set;
		using (new OVRObjectPool.HashSetScope<Guid>(out set))
		{
			List<OVRAnchor> anchors;
			using (new OVRObjectPool.ListScope<OVRAnchor>(out anchors))
			{
				set.Add(floor);
				set.Add(ceiling);
				Guid[] array = walls;
				foreach (Guid item in array)
				{
					set.Add(item);
				}
				if (_sceneAnchor.Anchor.TryGetComponent<OVRAnchorContainer>(out var component))
				{
					array = component.Uuids;
					foreach (Guid item2 in array)
					{
						set.Add(item2);
					}
				}
				if (!(await OVRSceneManager.FetchAnchorsAsync(set, anchors)))
				{
					return false;
				}
				List<bool> list;
				using (new OVRObjectPool.ListScope<bool>(out list))
				{
					List<OVRTask<bool>> list2;
					using (new OVRObjectPool.ListScope<OVRTask<bool>>(out list2))
					{
						foreach (OVRAnchor item3 in anchors)
						{
							if (item3.TryGetComponent<OVRLocatable>(out var component2))
							{
								list2.Add(component2.SetEnabledAsync(enabled: true));
							}
						}
						await OVRTask.WhenAll(list2, list);
					}
				}
				foreach (OVRAnchor item4 in anchors)
				{
					if (item4.TryGetComponent<OVRLocatable>(out var component3) && component3.IsEnabled)
					{
						OVRPlugin.GetSpaceComponentStatus(item4.Handle, OVRPlugin.SpaceComponentType.Bounded2D, out var flag, out var changePending);
						OVRPlugin.GetSpaceComponentStatus(item4.Handle, OVRPlugin.SpaceComponentType.Bounded3D, out var flag2, out changePending);
						OVRPlugin.GetSpaceComponentStatus(item4.Handle, OVRPlugin.SpaceComponentType.TriangleMesh, out var flag3, out changePending);
						OVRSceneAnchor prefab = ((flag && !(flag2 || flag3)) ? _sceneManager.PlanePrefab : (flag2 ? _sceneManager.VolumePrefab : null));
						OVRSceneAnchor oVRSceneAnchor = _sceneManager.InstantiateSceneAnchor(item4, prefab);
						if ((bool)oVRSceneAnchor)
						{
							oVRSceneAnchor.transform.parent = base.transform;
							oVRSceneAnchor.IsTracked = true;
						}
					}
				}
				Floor = GetPlane(floor);
				Ceiling = GetPlane(ceiling);
				List<OVRScenePlane> list3;
				using (new OVRObjectPool.ListScope<OVRScenePlane>(out list3))
				{
					array = walls;
					for (int i = 0; i < array.Length; i++)
					{
						if (TryGetPlane(array[i], out var plane2))
						{
							list3.Add(plane2);
						}
					}
					Walls = list3.ToArray();
				}
			}
		}
		return true;
		static OVRScenePlane GetPlane(Guid uuid)
		{
			if (!TryGetPlane(uuid, out var plane3))
			{
				return null;
			}
			return plane3;
		}
		static bool TryGetPlane(Guid uuid, out OVRScenePlane plane)
		{
			plane = null;
			if (OVRSceneAnchor.SceneAnchors.TryGetValue(uuid, out var value))
			{
				return value.TryGetComponent<OVRScenePlane>(out plane);
			}
			return false;
		}
	}

	private void OnDestroy()
	{
		SceneRooms.Remove(_uuid);
		SceneRoomsList.Remove(this);
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	private void Log(string message)
	{
		UnityEngine.Debug.Log("[OVRSceneRoom] " + message, base.gameObject);
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	private void LogWarning(string message)
	{
		UnityEngine.Debug.LogWarning("[OVRSceneRoom] " + message, base.gameObject);
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	private void LogError(string message)
	{
		UnityEngine.Debug.LogError("[OVRSceneRoom] " + message, base.gameObject);
	}
}
