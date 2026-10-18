using System;
using System.Reflection;

namespace Sirenix.OdinInspector;

public class OdinDesignerBindingAttribute : Attribute
{
	public string[] MemberNames;

	public OdinDesignerBindingAttribute(params string[] memberNames)
	{
		MemberNames = memberNames;
	}

	public MemberInfo GetBindingMemberInfo(Type type, int index)
	{
		FieldInfo field = type.GetField(MemberNames[index], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (field != null)
		{
			return field;
		}
		return type.GetProperty(MemberNames[index], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
	}
}
