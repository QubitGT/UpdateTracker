using System;
using System.Collections.Generic;
using System.Reflection;
using Meta.XR.ImmersiveDebugger.Gizmo;
using Meta.XR.ImmersiveDebugger.UserInterface;
using Meta.XR.ImmersiveDebugger.Utils;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.Manager;

internal class GizmoManagerForAddon : SubManagerForAddon
{
	private readonly Dictionary<MemberInfo, GizmoRendererManager> _memberToGizmoRendererManagerDict = new Dictionary<MemberInfo, GizmoRendererManager>();

	public override string TelemetryAnnotation => "Gizmos";

	protected override bool RegisterSpecialisedWidget(IMember member, MemberInfo memberInfo, DebugMember memberAttribute, InstanceHandle handle)
	{
		if (memberInfo.IsTypeEqual(typeof(Pose)))
		{
			memberAttribute.GizmoType = DebugGizmoType.Axis;
		}
		if (memberInfo.IsTypeEqual(typeof(Vector3)))
		{
			memberAttribute.GizmoType = DebugGizmoType.Point;
		}
		if (memberInfo.IsTypeEqual(typeof(Tuple<Vector3, Vector3>)))
		{
			memberAttribute.GizmoType = DebugGizmoType.Line;
		}
		if (memberInfo.IsTypeEqual(typeof(Vector3[])))
		{
			memberAttribute.GizmoType = DebugGizmoType.Lines;
		}
		if (memberInfo.IsTypeEqual(typeof(Tuple<Pose, float, float>)))
		{
			memberAttribute.GizmoType = DebugGizmoType.Plane;
		}
		if (memberInfo.IsTypeEqual(typeof(Tuple<Vector3, float>)))
		{
			memberAttribute.GizmoType = DebugGizmoType.Cube;
		}
		if (memberInfo.IsTypeEqual(typeof(Tuple<Pose, float, float, float>)))
		{
			memberAttribute.GizmoType = DebugGizmoType.Box;
		}
		if (memberAttribute.GizmoType == DebugGizmoType.None)
		{
			return false;
		}
		if (memberInfo.DeclaringType == typeof(Transform) && memberInfo.Name == "position")
		{
			memberAttribute.ShowGizmoByDefault = true;
		}
		if (!_memberToGizmoRendererManagerDict.TryGetValue(memberInfo, out var value) && GizmoManager.AddGizmo(handle.Type, memberInfo, memberAttribute, InstanceCache, out value))
		{
			_memberToGizmoRendererManagerDict[memberInfo] = value;
		}
		if (value == null)
		{
			return false;
		}
		GizmoHook gizmo = member.GetGizmo();
		if (gizmo == null || !gizmo.Matches(memberInfo, handle))
		{
			member.RegisterGizmo(new GizmoHook(memberInfo, handle, memberAttribute, OnStateChanged, GetState));
		}
		return true;
		bool GetState()
		{
			return _memberToGizmoRendererManagerDict[memberInfo].GetState(handle.Instance);
		}
		void OnStateChanged(bool state)
		{
			_memberToGizmoRendererManagerDict[memberInfo].SetState(handle.Instance, state);
		}
	}
}
