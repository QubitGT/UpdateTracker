using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlayFab.Internal;

public static class PlayFabUtil
{
	private static string _localSettingsFileName;

	public static readonly string[] _defaultDateTimeFormats;

	public const int DEFAULT_UTC_OUTPUT_INDEX = 2;

	public const int DEFAULT_LOCAL_OUTPUT_INDEX = 9;

	public static DateTimeStyles DateTimeStyles;

	[ThreadStatic]
	private static StringBuilder _sb;

	public static string timeStamp => DateTime.Now.ToString(_defaultDateTimeFormats[9]);

	public static string utcTimeStamp => DateTime.UtcNow.ToString(_defaultDateTimeFormats[2]);

	static PlayFabUtil()
	{
		_localSettingsFileName = "playfab.local.settings.json";
		_defaultDateTimeFormats = new string[15]
		{
			"yyyy-MM-ddTHH:mm:ss.FFFFFFZ", "yyyy-MM-ddTHH:mm:ss.FFFFZ", "yyyy-MM-ddTHH:mm:ss.FFFZ", "yyyy-MM-ddTHH:mm:ss.FFZ", "yyyy-MM-ddTHH:mm:ssZ", "yyyy-MM-dd HH:mm:ssZ", "yyyy-MM-dd HH:mm:ss.FFFFFF", "yyyy-MM-dd HH:mm:ss.FFFF", "yyyy-MM-dd HH:mm:ss.FFF", "yyyy-MM-dd HH:mm:ss.FF",
			"yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm.ss.FFFF", "yyyy-MM-dd HH:mm.ss.FFF", "yyyy-MM-dd HH:mm.ss.FF", "yyyy-MM-dd HH:mm.ss"
		};
		DateTimeStyles = DateTimeStyles.RoundtripKind;
	}

	public static string Format(string text, params object[] args)
	{
		if (args.Length == 0)
		{
			return text;
		}
		return string.Format(text, args);
	}

	public static string ReadAllFileText(string filename)
	{
		if (!File.Exists(filename))
		{
			return string.Empty;
		}
		if (_sb == null)
		{
			_sb = new StringBuilder();
		}
		_sb.Length = 0;
		using (FileStream input = new FileStream(filename, FileMode.Open))
		{
			using BinaryReader binaryReader = new BinaryReader(input);
			while (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
			{
				_sb.Append(binaryReader.ReadChar());
			}
		}
		return _sb.ToString();
	}

	public static T TryEnumParse<T>(string value, T defaultValue)
	{
		try
		{
			return (T)Enum.Parse(typeof(T), value);
		}
		catch (InvalidCastException)
		{
			return defaultValue;
		}
		catch (Exception ex2)
		{
			Debug.LogError("Enum cast failed with unknown error: " + ex2.Message);
			return defaultValue;
		}
	}

	internal static string GetLocalSettingsFileProperty(string propertyKey)
	{
		string text = null;
		string text2 = Path.Combine(Directory.GetCurrentDirectory(), _localSettingsFileName);
		if (File.Exists(text2))
		{
			text = ReadAllFileText(text2);
		}
		else
		{
			string text3 = Path.Combine(Path.GetTempPath(), _localSettingsFileName);
			if (File.Exists(text3))
			{
				text = ReadAllFileText(text3);
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			Dictionary<string, object> dictionary = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer).DeserializeObject<Dictionary<string, object>>(text);
			try
			{
				if (dictionary.TryGetValue(propertyKey, out var value))
				{
					return value?.ToString();
				}
				return null;
			}
			catch (KeyNotFoundException)
			{
				return string.Empty;
			}
		}
		return string.Empty;
	}
}
