using System;
using System.Collections.Generic;
using System.Reflection;
using Meta.XR.ImmersiveDebugger.Gizmo;
using Meta.XR.ImmersiveDebugger.Hierarchy;
using Meta.XR.ImmersiveDebugger.UserInterface;
using Meta.XR.ImmersiveDebugger.Utils;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.Manager;

internal class GizmoManager : IDebugManager
{
	internal readonly Dictionary<Type, List<(MemberInfo, GizmoRendererManager)>> GizmosDict = new Dictionary<Type, List<(MemberInfo, GizmoRendererManager)>>();

	private IDebugUIPanel _uiPanel;

	private InstanceCache _instanceCache;

	public string TelemetryAnnotation => "Gizmos";

	public void Setup(IDebugUIPanel panel, InstanceCache cache)
	{
		_uiPanel = panel;
		_instanceCache = cache;
	}

	public void ProcessType(Type type)
	{
		RemoveGizmosForType(type);
		GizmosDict.Remove(type);
		List<(MemberInfo, GizmoRendererManager)> gizmosList = new List<(MemberInfo, GizmoRendererManager)>();
		List<(MemberInfo, DebugMember)> membersList = new List<(MemberInfo, DebugMember)>();
		Dictionary<MemberInfo, GizmoRendererManager> memberToGizmoRendererManagerDict = new Dictionary<MemberInfo, GizmoRendererManager>();
		MemberInfo[] members = type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.GetProperty);
		foreach (MemberInfo memberInfo in members)
		{
			DebugMember customAttribute = memberInfo.GetCustomAttribute<DebugMember>();
			if (customAttribute != null && customAttribute.GizmoType != 0 && AddGizmo(type, memberInfo, customAttribute, _instanceCache, out var gizmoRendererManager))
			{
				gizmosList.Add((memberInfo, gizmoRendererManager));
				membersList.Add((memberInfo, customAttribute));
				memberToGizmoRendererManagerDict[memberInfo] = gizmoRendererManager;
			}
		}
		InspectedDataRegistry.GetMembersForType(type, delegate(MemberInfo info, DebugMember attribute)
		{
			if (attribute.GizmoType == DebugGizmoType.None)
			{
				return false;
			}
			if (!AddGizmo(type, info, attribute, _instanceCache, out var gizmoRendererManager2))
			{
				return false;
			}
			gizmosList.Add((info, gizmoRendererManager2));
			membersList.Add((info, attribute));
			memberToGizmoRendererManagerDict[info] = gizmoRendererManager2;
			return false;
		});
		GizmosDict[type] = gizmosList;
		ManagerUtils.RebuildInspectorForType(_uiPanel, _instanceCache, type, membersList, delegate(IMember memberController, MemberInfo member, DebugMember attribute, InstanceHandle instance)
		{
			GizmoHook gizmo = memberController.GetGizmo();
			if (gizmo == null || !gizmo.Matches(member, instance))
			{
				memberController.RegisterGizmo(new GizmoHook(member, instance, attribute, OnStateChanged, GetState));
			}
			bool GetState()
			{
				return memberToGizmoRendererManagerDict[member].GetState(instance.Instance);
			}
			void OnStateChanged(bool state)
			{
				memberToGizmoRendererManagerDict[member].SetState(instance.Instance, state);
			}
		});
	}

	public void ProcessTypeFromInspector(Type type, InstanceHandle handle, MemberInfo memberInfo, DebugMember memberAttribute)
	{
		throw new NotImplementedException();
	}

	public void ProcessTypeFromHierarchy(Item item, MemberInfo memberInfo)
	{
		throw new NotImplementedException();
	}

	internal static bool AddGizmo(Type type, MemberInfo member, DebugMember gizmoAttribute, InstanceCache instanceCache, out GizmoRendererManager gizmoRendererManager)
	{
		if (!GizmoTypesRegistry.IsValidDataTypeForGizmoType(member.GetDataType(), gizmoAttribute.GizmoType))
		{
			Debug.LogWarning("Invalid registration of gizmo " + member.Name + ": type not matching gizmo type");
			gizmoRendererManager = null;
			return false;
		}
		GameObject gameObject = new GameObject(member.Name + "Gizmo");
		gizmoRendererManager = gameObject.AddComponent<GizmoRendererManager>();
		gizmoRendererManager.Setup(type, member, gizmoAttribute.GizmoType, gizmoAttribute.Color, instanceCache);
		if (Application.isPlaying)
		{
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
		}
		return true;
	}

	private void RemoveGizmosForType(Type type)
	{
		if (!GizmosDict.TryGetValue(type, out var value))
		{
			return;
		}
		foreach (var item in value)
		{
			UnityEngine.Object.Destroy(item.Item2.gameObject);
		}
		GizmosDict.Remove(type);
	}

	public int GetCountPerType(Type type)
	{
		GizmosDict.TryGetValue(type, out var value);
		return value?.Count ?? 0;
	}
}
