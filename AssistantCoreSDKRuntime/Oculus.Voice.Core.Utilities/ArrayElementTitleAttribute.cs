using UnityEngine;

namespace Oculus.Voice.Core.Utilities;

public class ArrayElementTitleAttribute : PropertyAttribute
{
	public string varname;

	public string fallbackName;

	public ArrayElementTitleAttribute(string elementTitleVar = null, string fallbackName = null)
	{
		varname = elementTitleVar;
		this.fallbackName = fallbackName;
	}
}
