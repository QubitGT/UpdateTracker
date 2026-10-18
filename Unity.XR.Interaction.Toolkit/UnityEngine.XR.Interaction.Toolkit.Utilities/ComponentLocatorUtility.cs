namespace UnityEngine.XR.Interaction.Toolkit.Utilities;

internal static class ComponentLocatorUtility<T> where T : Component
{
	private static T s_ComponentCache;

	private static int s_LastTryFindFrame = -1;

	internal static T componentCache => s_ComponentCache;

	private static bool FindWasPerformedThisFrame()
	{
		return s_LastTryFindFrame == Time.frameCount;
	}

	public static T FindOrCreateComponent()
	{
		if (s_ComponentCache == null)
		{
			s_ComponentCache = Find();
			if (s_ComponentCache == null)
			{
				s_ComponentCache = new GameObject(typeof(T).Name, typeof(T)).GetComponent<T>();
			}
		}
		return s_ComponentCache;
	}

	public static T FindComponent()
	{
		TryFindComponent(out var component);
		return component;
	}

	public static bool TryFindComponent(out T component)
	{
		if (s_ComponentCache != null)
		{
			component = s_ComponentCache;
			return true;
		}
		s_ComponentCache = Find();
		component = s_ComponentCache;
		return component != null;
	}

	internal static bool TryFindComponent(out T component, bool limitTryFindPerFrame)
	{
		if (limitTryFindPerFrame && FindWasPerformedThisFrame() && s_ComponentCache == null)
		{
			component = null;
			return false;
		}
		return TryFindComponent(out component);
	}

	private static T Find()
	{
		s_LastTryFindFrame = Time.frameCount;
		return Object.FindFirstObjectByType<T>();
	}
}
