using System.Reflection;
using UnityEngine.SceneManagement;

namespace UnityEngine.Rendering;

internal static class SceneExtensions
{
	private static PropertyInfo s_SceneGUID = typeof(Scene).GetProperty("guid", BindingFlags.Instance | BindingFlags.NonPublic);

	public static string GetGUID(this Scene scene)
	{
		return (string)s_SceneGUID.GetValue(scene);
	}
}
