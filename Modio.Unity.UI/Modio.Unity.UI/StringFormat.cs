using System;
using Modio.Unity.UI.Components.Localization;
using UnityEngine;

namespace Modio.Unity.UI;

internal static class StringFormat
{
	public const string BYTES_FORMAT_TOOLTIP = "Bytes: \"1048576\".\r\nBytesComma: \"1,048,576\".\r\nSuffix: \"1 MB\".";

	private static readonly string[] BytesSuffixes = new string[4] { "B", "KB", "MB", "GB" };

	private static readonly string[] BytesSuffixesLoc = new string[4] { "modio_unit_bytes", "modio_unit_kb", "modio_unit_mb", "modio_unit_gb" };

	public const string KILO_FORMAT_TOOLTIP = "None: \"10500\".\r\nComma: \"10,500\".\r\nKilo: \"10.5k\".";

	public static string Bytes(StringFormatBytes format, long bytes, string custom = null, bool reducePrecision = false)
	{
		return format switch
		{
			StringFormatBytes.Bytes => bytes.ToString(), 
			StringFormatBytes.BytesComma => BytesComma(bytes), 
			StringFormatBytes.Suffix => BytesSuffix(bytes, reducePrecision), 
			StringFormatBytes.Custom => string.IsNullOrEmpty(custom) ? bytes.ToString(ModioUILocalizationManager.CultureInfo) : bytes.ToString(custom, ModioUILocalizationManager.CultureInfo), 
			_ => bytes.ToString(), 
		};
	}

	public static string BytesComma(long bytes)
	{
		return bytes.ToString("N0", ModioUILocalizationManager.CultureInfo);
	}

	public static string BytesSuffix(long bytes, bool reducePrecision = false)
	{
		int num = Mathf.Clamp((int)Math.Log(bytes, 1024.0), 1, BytesSuffixes.Length - 1);
		double num2 = (double)bytes / Math.Pow(1024.0, num);
		string text = BytesSuffixes[num];
		text = ModioUILocalizationManager.GetLocalizedText(BytesSuffixesLoc[num], errorIfMissing: false) ?? text;
		int num3 = (reducePrecision ? Mathf.Max(0, 2 - (int)Mathf.Log10(bytes)) : 2);
		return num2.ToString($"F{num3}") + " " + text;
	}

	public static string Kilo(StringFormatKilo format, long value, string custom = null)
	{
		return format switch
		{
			StringFormatKilo.None => value.ToString(), 
			StringFormatKilo.Comma => value.ToString("N0"), 
			StringFormatKilo.Kilo => Kilo(value), 
			StringFormatKilo.Custom => string.IsNullOrEmpty(custom) ? value.ToString() : value.ToString(custom), 
			_ => value.ToString(), 
		};
	}

	public static string Kilo(long value)
	{
		if (value > 1000000000000L)
		{
			return ((double)value / 1000000000000.0).ToString("0.#T");
		}
		if (value > 100000000000L)
		{
			return ((double)value / 1000000000000.0).ToString("0.##T");
		}
		if (value > 10000000000L)
		{
			return ((double)value / 1000000000.0).ToString("0.#G");
		}
		if (value > 1000000000)
		{
			return ((double)value / 1000000000.0).ToString("0.##G");
		}
		if (value > 100000000)
		{
			return ((double)value / 1000000.0).ToString("0.#M");
		}
		if (value > 1000000)
		{
			return ((double)value / 1000000.0).ToString("0.##M");
		}
		if (value > 100000)
		{
			return ((double)value / 1000.0).ToString("0.#k");
		}
		if (value > 10000)
		{
			return ((double)value / 1000.0).ToString("0.##k");
		}
		return value.ToString("#,0");
	}
}
