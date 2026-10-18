using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine.Analytics;

namespace UnityEngine.XR.Management;

internal static class XRManagementAnalytics
{
	[Serializable]
	private struct BuildEvent : IAnalytic.IData
	{
		public string buildGuid;

		public string buildTarget;

		public string buildTargetGroup;

		public string[] assigned_loaders;
	}

	[AnalyticInfo("xrmanagment_build", "unity.xrmanagement", 1, 1000, 1000)]
	private class XrInitializeAnalytic : IAnalytic
	{
		private BuildEvent? data;

		public XrInitializeAnalytic(BuildEvent data)
		{
			this.data = data;
		}

		public bool TryGatherData(out IAnalytic.IData data, [NotNullWhen(false)] out Exception error)
		{
			error = null;
			data = this.data;
			return data != null;
		}
	}

	private const int kMaxEventsPerHour = 1000;

	private const int kMaxNumberOfElements = 1000;

	private const string kVendorKey = "unity.xrmanagement";

	private const string kEventBuild = "xrmanagment_build";

	private static bool s_Initialized;

	private static bool Initialize()
	{
		return s_Initialized;
	}
}
