using System;
using System.Collections.Generic;
using System.Text;

namespace Unity.Cinemachine;

internal static class CinemachineDebug
{
	private static List<StringBuilder> s_AvailableStringBuilders;

	public static Action<CinemachineBrain> OnGUIHandlers;

	public static bool GameViewGuidesEnabled;

	public static StringBuilder SBFromPool()
	{
		if (s_AvailableStringBuilders == null || s_AvailableStringBuilders.Count == 0)
		{
			return new StringBuilder();
		}
		List<StringBuilder> list = s_AvailableStringBuilders;
		StringBuilder stringBuilder = list[list.Count - 1];
		s_AvailableStringBuilders.RemoveAt(s_AvailableStringBuilders.Count - 1);
		stringBuilder.Length = 0;
		return stringBuilder;
	}

	public static void ReturnToPool(StringBuilder sb)
	{
		if (s_AvailableStringBuilders == null)
		{
			s_AvailableStringBuilders = new List<StringBuilder>();
		}
		s_AvailableStringBuilders.Add(sb);
	}
}
