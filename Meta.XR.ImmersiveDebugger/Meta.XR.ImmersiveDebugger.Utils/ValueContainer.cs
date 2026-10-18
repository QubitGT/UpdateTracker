using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.Utils;

internal class ValueContainer<T> : ScriptableObject
{
	public ValueStruct<T>[] Values;

	private static string Path => "Values/";

	public T this[string valueName] => GetValue(valueName);

	public static ValueContainer<T> Load(string assetName)
	{
		return Resources.Load<ValueContainer<T>>(Path + assetName);
	}

	public T GetValue(string valueName)
	{
		ValueStruct<T>[] values = Values;
		for (int i = 0; i < values.Length; i++)
		{
			ValueStruct<T> valueStruct = values[i];
			if (valueStruct.ValueName.Equals(valueName))
			{
				return valueStruct.Value;
			}
		}
		Debug.LogWarning("Value " + valueName + " not found in " + base.name + ".");
		return default(T);
	}
}
