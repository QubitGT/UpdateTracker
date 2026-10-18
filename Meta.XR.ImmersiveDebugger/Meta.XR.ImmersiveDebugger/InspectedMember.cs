using System;
using System.Collections.Generic;
using System.Reflection;
using Meta.XR.ImmersiveDebugger.Utils;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger;

[Serializable]
internal class InspectedMember : InspectedItemBase
{
	internal const BindingFlags Flags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	[SerializeField]
	public DebugMember attribute;

	[SerializeField]
	private string memberName;

	[SerializeField]
	internal int _editorSelectedGizmoIndex;

	internal List<DebugGizmoType> SupportedGizmos { get; private set; }

	public MemberInfo MemberInfo { get; private set; }

	public InspectedMember(MemberInfo member)
	{
		enabled = false;
		typeName = member.DeclaringType?.AssemblyQualifiedName;
		memberName = member.Name;
		attribute = new DebugMember();
		Initialize();
	}

	public void Initialize()
	{
		base.Valid = false;
		Type type = Type.GetType(typeName);
		if (!(type == null))
		{
			MemberInfo[] member = type.GetMember(memberName, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (member.Length != 0)
			{
				MemberInfo = member[0];
				base.Valid = true;
				SupportedGizmos = new List<DebugGizmoType>();
				PopulateSupportedGizmos(SupportedGizmos);
			}
		}
	}

	private void PopulateSupportedGizmos(List<DebugGizmoType> supportedGizmos)
	{
		if (supportedGizmos == null)
		{
			throw new NullReferenceException("supportedGizmos array cannot be null");
		}
		supportedGizmos.Add(DebugGizmoType.None);
		if (MemberInfo.IsTypeEqual(typeof(Pose)))
		{
			supportedGizmos.Add(DebugGizmoType.Axis);
		}
		if (MemberInfo.IsTypeEqual(typeof(Vector3)))
		{
			supportedGizmos.Add(DebugGizmoType.Point);
		}
		if (MemberInfo.IsTypeEqual(typeof(Tuple<Vector3, Vector3>)))
		{
			supportedGizmos.Add(DebugGizmoType.Line);
		}
		if (MemberInfo.IsTypeEqual(typeof(Vector3[])))
		{
			supportedGizmos.Add(DebugGizmoType.Lines);
		}
		if (MemberInfo.IsTypeEqual(typeof(Tuple<Pose, float, float>)))
		{
			supportedGizmos.Add(DebugGizmoType.Plane);
		}
		if (MemberInfo.IsTypeEqual(typeof(Tuple<Vector3, float>)))
		{
			supportedGizmos.Add(DebugGizmoType.Cube);
		}
		if (MemberInfo.IsTypeEqual(typeof(Tuple<Pose, float, float, float>)))
		{
			supportedGizmos.Add(DebugGizmoType.TopCenterBox);
			supportedGizmos.Add(DebugGizmoType.Box);
		}
	}
}
