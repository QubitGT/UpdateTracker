using System;

namespace UnityEngine.Rendering;

internal class ProbeVolumeDebugColorPreferences
{
	internal static Func<Color> GetDetailSubdivisionColor;

	internal static Func<Color> GetMediumSubdivisionColor;

	internal static Func<Color> GetLowSubdivisionColor;

	internal static Func<Color> GetVeryLowSubdivisionColor;

	internal static Func<Color> GetSparseSubdivisionColor;

	internal static Func<Color> GetSparsestSubdivisionColor;

	internal static Color s_DetailSubdivision;

	internal static Color s_MediumSubdivision;

	internal static Color s_LowSubdivision;

	internal static Color s_VeryLowSubdivision;

	internal static Color s_SparseSubdivision;

	internal static Color s_SparsestSubdivision;

	static ProbeVolumeDebugColorPreferences()
	{
		s_DetailSubdivision = new Color32(135, 35, byte.MaxValue, byte.MaxValue);
		s_MediumSubdivision = new Color32(54, 208, 228, byte.MaxValue);
		s_LowSubdivision = new Color32(byte.MaxValue, 100, 45, byte.MaxValue);
		s_VeryLowSubdivision = new Color32(52, 87, byte.MaxValue, byte.MaxValue);
		s_SparseSubdivision = new Color32(byte.MaxValue, 71, 97, byte.MaxValue);
		s_SparsestSubdivision = new Color32(200, 227, 39, byte.MaxValue);
	}
}
