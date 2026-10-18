using UnityEngine;

public sealed class MetaXRAudioSettings : ScriptableObject
{
	[SerializeField]
	public int voiceLimit = 64;

	private static MetaXRAudioSettings instance;

	public static MetaXRAudioSettings Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Resources.Load<MetaXRAudioSettings>("MetaXRAudioSettings");
				if (instance == null)
				{
					instance = ScriptableObject.CreateInstance<MetaXRAudioSettings>();
				}
			}
			return instance;
		}
	}
}
