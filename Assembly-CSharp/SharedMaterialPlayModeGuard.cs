using System.Diagnostics;
using UnityEngine;

public static class SharedMaterialPlayModeGuard
{
	[Conditional("UNITY_EDITOR")]
	public static void Remember(Material material)
	{
	}
}
