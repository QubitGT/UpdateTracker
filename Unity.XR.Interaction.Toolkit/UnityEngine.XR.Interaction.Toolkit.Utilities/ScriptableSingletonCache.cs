using System.Collections.Generic;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities;

internal static class ScriptableSingletonCache<T> where T : ScriptableObject
{
	private static T s_Instance;

	private static readonly Dictionary<ScriptableObject, HashSet<object>> s_UsersPerInstance = new Dictionary<ScriptableObject, HashSet<object>>();

	public static T GetInstance(object user)
	{
		if (s_Instance == null)
		{
			s_Instance = ScriptableObject.CreateInstance<T>();
		}
		if (!s_UsersPerInstance.TryGetValue(s_Instance, out var value))
		{
			value = new HashSet<object>();
			s_UsersPerInstance.Add(s_Instance, value);
		}
		value.Add(user);
		return s_Instance;
	}

	public static void ReleaseInstance(object user)
	{
		if (s_Instance == null)
		{
			return;
		}
		if (!s_UsersPerInstance.TryGetValue(s_Instance, out var value))
		{
			Object.Destroy(s_Instance);
			return;
		}
		value.Remove(user);
		if (value.Count == 0)
		{
			s_UsersPerInstance.Remove(s_Instance);
			Object.Destroy(s_Instance);
		}
	}
}
