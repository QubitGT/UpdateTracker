using UnityEngine;

namespace Meta.XR.BuildingBlocks;

public static class RunTimeUtils
{
	public static T GetInterfaceComponent<T>(this MonoBehaviour monoBehaviour) where T : class
	{
		MonoBehaviour[] components = monoBehaviour.GetComponents<MonoBehaviour>();
		for (int i = 0; i < components.Length; i++)
		{
			if (components[i] is T result)
			{
				return result;
			}
		}
		return null;
	}

	public static string GenerateRandomString(int size, bool includeLowercase = true, bool includeUppercase = true, bool includeNumeric = true, bool includeSpecial = false)
	{
		string text = "";
		if (includeLowercase)
		{
			text += "abcdefghijklmnopqrstuvwxyz";
		}
		if (includeUppercase)
		{
			text += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		}
		if (includeNumeric)
		{
			text += "0123456789";
		}
		if (includeSpecial)
		{
			text += "!@#$%^&*()_-+=[{]};:<>|./?";
		}
		char[] array = new char[size];
		for (int i = 0; i < size; i++)
		{
			array[i] = text[Random.Range(0, text.Length)];
		}
		return new string(array);
	}
}
