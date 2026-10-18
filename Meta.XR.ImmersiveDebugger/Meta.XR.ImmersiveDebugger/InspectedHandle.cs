using System;
using System.Collections.Generic;
using System.Reflection;
using Meta.XR.ImmersiveDebugger.Utils;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger;

[Serializable]
internal class InspectedHandle : InspectedItemBase
{
	[SerializeField]
	public List<InspectedMember> inspectedMembers = new List<InspectedMember>();

	public InstanceHandle InstanceHandle { get; private set; }

	public Type Type { get; private set; }

	public InspectedHandle(DebugInspector owner, Type type)
	{
		enabled = false;
		typeName = type.AssemblyQualifiedName;
		Initialize(owner);
	}

	public void Initialize(DebugInspector owner)
	{
		base.Valid = false;
		Type = Type.GetType(typeName);
		if (Type == null)
		{
			return;
		}
		Component component = owner.GetComponent(Type);
		if (component == null)
		{
			return;
		}
		InstanceHandle = new InstanceHandle(Type, component);
		foreach (InspectedMember inspectedMember2 in inspectedMembers)
		{
			inspectedMember2.Initialize();
		}
		Type type = Type;
		while (type != null && type != typeof(Component) && type != typeof(MonoBehaviour))
		{
			MemberInfo[] members = type.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MemberInfo memberInfo in members)
			{
				if (!TryGetMember(memberInfo, out var inspectedMember) && memberInfo.IsCompatibleWithDebugInspector())
				{
					inspectedMember = new InspectedMember(memberInfo);
					inspectedMembers.Add(inspectedMember);
				}
			}
			type = type.BaseType;
		}
		base.Valid = true;
	}

	private bool TryGetMember(MemberInfo memberInfo, out InspectedMember inspectedMember)
	{
		inspectedMember = null;
		foreach (InspectedMember inspectedMember2 in inspectedMembers)
		{
			if (inspectedMember2.MemberInfo == memberInfo)
			{
				inspectedMember = inspectedMember2;
				break;
			}
		}
		return inspectedMember != null;
	}
}
