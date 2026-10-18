using System;
using System.Linq;
using System.Reflection;

namespace UnityEngine.Rendering;

public static class DocumentationUtils
{
	public static string GetHelpURL<TEnum>(TEnum mask = default(TEnum)) where TEnum : struct, IConvertible
	{
		HelpURLAttribute helpURLAttribute = (HelpURLAttribute)mask.GetType().GetCustomAttributes(typeof(HelpURLAttribute), inherit: false).FirstOrDefault();
		if (helpURLAttribute != null)
		{
			return $"{helpURLAttribute.URL}#{mask}";
		}
		return string.Empty;
	}

	public static bool TryGetHelpURL(Type type, out string url)
	{
		HelpURLAttribute customAttribute = type.GetCustomAttribute<HelpURLAttribute>(inherit: false);
		url = customAttribute?.URL;
		return customAttribute != null;
	}
}
