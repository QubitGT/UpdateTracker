using UnityEngine;

namespace Unity.XR.CoreUtils;

public abstract class ScriptableSettings<T> : ScriptableSettingsBase<T> where T : ScriptableObject
{
	private const string k_CustomSavePathFormat = "{0}Resources/{1}.asset";

	private const string k_SavePathFormat = "{0}Resources/ScriptableSettings/{1}.asset";

	private const string k_LoadPathFormat = "ScriptableSettings/{0}";

	public static T Instance
	{
		get
		{
			if (ScriptableSettingsBase<T>.BaseInstance == null)
			{
				CreateAndLoad();
			}
			return ScriptableSettingsBase<T>.BaseInstance;
		}
	}

	internal static T CreateAndLoad()
	{
		ScriptableSettingsBase<T>.BaseInstance = Resources.Load(ScriptableSettingsBase<T>.HasCustomPath ? ScriptableSettingsBase<T>.GetFilePath() : $"ScriptableSettings/{ScriptableSettingsBase<T>.GetFilePath()}") as T;
		if (ScriptableSettingsBase<T>.BaseInstance == null)
		{
			ScriptableSettingsBase<T>.BaseInstance = ScriptableObject.CreateInstance<T>();
			ScriptableSettingsBase<T>.Save(ScriptableSettingsBase<T>.HasCustomPath ? "{0}Resources/{1}.asset" : "{0}Resources/ScriptableSettings/{1}.asset");
		}
		return ScriptableSettingsBase<T>.BaseInstance;
	}
}
