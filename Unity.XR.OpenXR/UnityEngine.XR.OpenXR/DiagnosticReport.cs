using System;
using System.Runtime.InteropServices;

namespace UnityEngine.XR.OpenXR;

internal class DiagnosticReport
{
	private const string LibraryName = "UnityOpenXR";

	public static readonly ulong k_NullSection;

	[DllImport("UnityOpenXR", EntryPoint = "DiagnosticReport_StartReport")]
	public static extern void StartReport();

	[DllImport("UnityOpenXR", CharSet = CharSet.Ansi, EntryPoint = "DiagnosticReport_GetSection")]
	public static extern ulong GetSection(string sectionName);

	[DllImport("UnityOpenXR", CharSet = CharSet.Ansi, EntryPoint = "DiagnosticReport_AddSectionEntry")]
	public static extern void AddSectionEntry(ulong sectionHandle, string sectionEntry, string sectionBody);

	[DllImport("UnityOpenXR", CharSet = CharSet.Ansi, EntryPoint = "DiagnosticReport_AddSectionBreak")]
	public static extern void AddSectionBreak(ulong sectionHandle);

	[DllImport("UnityOpenXR", CharSet = CharSet.Ansi, EntryPoint = "DiagnosticReport_AddEventEntry")]
	public static extern void AddEventEntry(string eventName, string eventData);

	[DllImport("UnityOpenXR", EntryPoint = "DiagnosticReport_DumpReport")]
	private static extern void Internal_DumpReport();

	[DllImport("UnityOpenXR", EntryPoint = "DiagnosticReport_DumpReportWithReason")]
	private static extern void Internal_DumpReport(string reason);

	[DllImport("UnityOpenXR", EntryPoint = "DiagnosticReport_GenerateReport")]
	private static extern IntPtr Internal_GenerateReport();

	[DllImport("UnityOpenXR", EntryPoint = "DiagnosticReport_ReleaseReport")]
	private static extern void Internal_ReleaseReport(IntPtr report);

	internal static string GenerateReport()
	{
		string result = "";
		IntPtr intPtr = Internal_GenerateReport();
		if (intPtr != IntPtr.Zero)
		{
			result = Marshal.PtrToStringAnsi(intPtr);
			Internal_ReleaseReport(intPtr);
			intPtr = IntPtr.Zero;
		}
		return result;
	}

	public static void DumpReport(string reason)
	{
		Internal_DumpReport(reason);
	}
}
