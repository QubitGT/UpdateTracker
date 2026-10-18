using System;
using UnityEngine;

namespace Unity.XR.CoreUtils.GUI;

public sealed class EnumDisplayAttribute : PropertyAttribute
{
	public string[] Names;

	public int[] Values;

	public EnumDisplayAttribute(params object[] enumValues)
	{
		Names = new string[enumValues.Length];
		Values = new int[enumValues.Length];
		int num = 0;
		while (num < Values.Length)
		{
			if (!(enumValues[num] is Enum @enum))
			{
				Debug.LogError($"Non-enum passed into EnumDisplay Attribute: {enumValues[num]}");
				continue;
			}
			Names[num] = @enum.ToString();
			Values[num] = Convert.ToInt32(@enum);
			num++;
		}
	}
}
