namespace UnityEngine.Rendering;

public class DebugDisplaySettingsHDROutput
{
	private static class Strings
	{
		public static readonly string hdrOutputAPI = "HDROutputSettings";

		public static readonly string displayName = "Display ";

		public static readonly string displayMain = " (main)";

		public static readonly string hdrActive = "HDR Output Active";

		public static readonly string hdrAvailable = "HDR Output Available";

		public static readonly string gamut = "Display Color Gamut";

		public static readonly string format = "Display Buffer Graphics Format";

		public static readonly string autoHdrTonemapping = "Automatic HDR Tonemapping";

		public static readonly string paperWhite = "Paper White Nits";

		public static readonly string minLuminance = "Min Tone Map Luminance";

		public static readonly string maxLuminance = "Max Tone Map Luminance";

		public static readonly string maxFullFrameLuminance = "Max Full Frame Tone Map Luminance";

		public static readonly string modeChangeRequested = "HDR Mode Change Requested";

		public static readonly string notAvailable = "N/A";
	}

	public static DebugUI.Table CreateHDROuputDisplayTable()
	{
		DebugUI.Table table = new DebugUI.Table
		{
			displayName = Strings.hdrOutputAPI,
			isReadOnly = true
		};
		DebugUI.Table.Row row = new DebugUI.Table.Row
		{
			displayName = Strings.hdrActive,
			opened = true
		};
		DebugUI.Table.Row row2 = new DebugUI.Table.Row
		{
			displayName = Strings.hdrAvailable,
			opened = true
		};
		DebugUI.Table.Row row3 = new DebugUI.Table.Row
		{
			displayName = Strings.gamut,
			opened = false
		};
		DebugUI.Table.Row row4 = new DebugUI.Table.Row
		{
			displayName = Strings.format,
			opened = false
		};
		DebugUI.Table.Row row5 = new DebugUI.Table.Row
		{
			displayName = Strings.autoHdrTonemapping,
			opened = false
		};
		DebugUI.Table.Row row6 = new DebugUI.Table.Row
		{
			displayName = Strings.paperWhite,
			opened = false
		};
		DebugUI.Table.Row row7 = new DebugUI.Table.Row
		{
			displayName = Strings.minLuminance,
			opened = false
		};
		DebugUI.Table.Row row8 = new DebugUI.Table.Row
		{
			displayName = Strings.maxLuminance,
			opened = false
		};
		DebugUI.Table.Row row9 = new DebugUI.Table.Row
		{
			displayName = Strings.maxFullFrameLuminance,
			opened = false
		};
		DebugUI.Table.Row row10 = new DebugUI.Table.Row
		{
			displayName = Strings.modeChangeRequested,
			opened = false
		};
		HDROutputSettings[] displays = HDROutputSettings.displays;
		for (int i = 0; i < displays.Length; i++)
		{
			HDROutputSettings d = displays[i];
			string text = Strings.displayName + (i + 1);
			if (HDROutputSettings.main == d)
			{
				text += Strings.displayMain;
			}
			row.children.Add(new DebugUI.Value
			{
				displayName = text,
				getter = () => d.active
			});
			row2.children.Add(new DebugUI.Value
			{
				displayName = text,
				getter = () => d.available
			});
			row3.children.Add(new DebugUI.Value
			{
				displayName = text,
				getter = () => d.available ? ((object)d.displayColorGamut) : Strings.notAvailable
			});
			row4.children.Add(new DebugUI.Value
			{
				displayName = text,
				getter = () => d.available ? ((object)d.graphicsFormat) : Strings.notAvailable
			});
			row5.children.Add(new DebugUI.Value
			{
				displayName = text,
				getter = () => d.available ? ((object)d.automaticHDRTonemapping) : Strings.notAvailable
			});
			row6.children.Add(new DebugUI.Value
			{
				displayName = text,
				getter = () => d.available ? ((object)d.paperWhiteNits) : Strings.notAvailable
			});
			row7.children.Add(new DebugUI.Value
			{
				displayName = text,
				getter = () => d.available ? ((object)d.minToneMapLuminance) : Strings.notAvailable
			});
			row8.children.Add(new DebugUI.Value
			{
				displayName = text,
				getter = () => d.available ? ((object)d.maxToneMapLuminance) : Strings.notAvailable
			});
			row9.children.Add(new DebugUI.Value
			{
				displayName = text,
				getter = () => d.available ? ((object)d.maxFullFrameToneMapLuminance) : Strings.notAvailable
			});
			row10.children.Add(new DebugUI.Value
			{
				displayName = text,
				getter = () => d.available ? ((object)d.HDRModeChangeRequested) : Strings.notAvailable
			});
		}
		table.children.Add(row);
		table.children.Add(row2);
		table.children.Add(row3);
		table.children.Add(row4);
		table.children.Add(row5);
		table.children.Add(row6);
		table.children.Add(row7);
		table.children.Add(row8);
		table.children.Add(row9);
		table.children.Add(row10);
		return table;
	}
}
