using System;
using UnityEngine;

namespace Drawing;

public class DrawingSettings : ScriptableObject
{
	[Serializable]
	public class Settings
	{
		public float lineOpacity = 1f;

		public float solidOpacity = 0.55f;

		public float textOpacity = 1f;

		public float lineOpacityBehindObjects = 0.12f;

		public float solidOpacityBehindObjects = 0.45f;

		public float textOpacityBehindObjects = 0.9f;

		public float curveResolution = 1f;
	}

	public const string SettingsPathCompatibility = "Assets/Settings/ALINE.asset";

	public const string SettingsName = "ALINE";

	public const string SettingsPath = "Assets/Settings/Resources/ALINE.asset";

	[SerializeField]
	private int version;

	public Settings settings;

	public static Settings DefaultSettings => new Settings();

	public static DrawingSettings GetSettingsAsset()
	{
		return Resources.Load<DrawingSettings>("ALINE");
	}
}
