using System;
using System.Runtime.CompilerServices;

namespace VYaml.Internal;

internal static class TypeHelper
{
	public static bool IsAnonymous(Type type)
	{
		if (type.Namespace == null && type.IsSealed && (type.Name.StartsWith("<>f__AnonymousType", StringComparison.Ordinal) || type.Name.StartsWith("<>__AnonType", StringComparison.Ordinal) || type.Name.StartsWith("VB$AnonymousType_", StringComparison.Ordinal)))
		{
			return type.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false);
		}
		return false;
	}
}
