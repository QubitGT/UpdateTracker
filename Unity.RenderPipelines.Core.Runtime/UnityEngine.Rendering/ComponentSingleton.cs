namespace UnityEngine.Rendering;

public static class ComponentSingleton<TType> where TType : Component
{
	private static TType s_Instance;

	public static TType instance
	{
		get
		{
			if (s_Instance == null)
			{
				GameObject obj = new GameObject("Default " + typeof(TType).Name)
				{
					hideFlags = HideFlags.HideAndDontSave
				};
				Object.DontDestroyOnLoad(obj);
				obj.SetActive(value: false);
				s_Instance = obj.AddComponent<TType>();
			}
			return s_Instance;
		}
	}

	public static void Release()
	{
		if (s_Instance != null)
		{
			CoreUtils.Destroy(s_Instance.gameObject);
			s_Instance = null;
		}
	}
}
