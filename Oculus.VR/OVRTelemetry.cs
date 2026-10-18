using System;
using UnityEngine;

internal static class OVRTelemetry
{
	public readonly struct MarkerPoint : IDisposable
	{
		public int NameHandle { get; }

		public MarkerPoint(string name)
		{
			Client.CreateMarkerHandle(name, out var nameHandle);
			NameHandle = nameHandle;
		}

		public void Dispose()
		{
			Client.DestroyMarkerHandle(NameHandle);
		}
	}

	public abstract class TelemetryClient
	{
		public abstract void MarkerStart(int markerId, int instanceKey = 0, long timestampMs = -1L, string joinId = null);

		public abstract void MarkerPointCached(int markerId, int nameHandle, int instanceKey = 0, long timestampMs = -1L);

		public abstract void MarkerPoint(int markerId, string name, int instanceKey = 0, long timestampMs = -1L);

		public unsafe abstract void MarkerPoint(int markerId, string name, OVRPlugin.Qpl.Annotation* annotations, int annotationCount, int instanceKey = 0, long timestampMs = -1L);

		public abstract void MarkerAnnotation(int markerId, string key, OVRPlugin.Qpl.Variant value, int instanceKey = 0);

		public abstract void MarkerAnnotation(int markerId, string annotationKey, string annotationValue, int instanceKey = 0);

		public void MarkerAnnotation(int markerId, string annotationKey, bool annotationValue, int instanceKey = 0)
		{
			MarkerAnnotation(markerId, annotationKey, OVRPlugin.Qpl.Variant.From(annotationValue), instanceKey);
		}

		public void MarkerAnnotation(int markerId, string annotationKey, long annotationValue, int instanceKey = 0)
		{
			MarkerAnnotation(markerId, annotationKey, OVRPlugin.Qpl.Variant.From(annotationValue), instanceKey);
		}

		public void MarkerAnnotation(int markerId, string annotationKey, double annotationValue, int instanceKey = 0)
		{
			MarkerAnnotation(markerId, annotationKey, OVRPlugin.Qpl.Variant.From(annotationValue), instanceKey);
		}

		public unsafe void MarkerAnnotation(int markerId, string annotationKey, byte** annotationValues, int count, int instanceKey = 0)
		{
			MarkerAnnotation(markerId, annotationKey, OVRPlugin.Qpl.Variant.From(annotationValues, count), instanceKey);
		}

		public unsafe void MarkerAnnotation(int markerId, string annotationKey, long* annotationValues, int count, int instanceKey = 0)
		{
			MarkerAnnotation(markerId, annotationKey, OVRPlugin.Qpl.Variant.From(annotationValues, count), instanceKey);
		}

		public unsafe void MarkerAnnotation(int markerId, string annotationKey, double* annotationValues, int count, int instanceKey = 0)
		{
			MarkerAnnotation(markerId, annotationKey, OVRPlugin.Qpl.Variant.From(annotationValues, count), instanceKey);
		}

		public unsafe void MarkerAnnotation(int markerId, string annotationKey, OVRPlugin.Bool* annotationValues, int count, int instanceKey = 0)
		{
			MarkerAnnotation(markerId, annotationKey, OVRPlugin.Qpl.Variant.From(annotationValues, count), instanceKey);
		}

		public abstract void MarkerEnd(int markerId, OVRPlugin.Qpl.ResultType resultTypeId = OVRPlugin.Qpl.ResultType.Success, int instanceKey = 0, long timestampMs = -1L);

		public abstract bool CreateMarkerHandle(string name, out int nameHandle);

		public abstract bool DestroyMarkerHandle(int nameHandle);
	}

	private class NullTelemetryClient : TelemetryClient
	{
		public override void MarkerStart(int markerId, int instanceKey = 0, long timestampMs = -1L, string joinId = null)
		{
		}

		public override void MarkerPointCached(int markerId, int nameHandle, int instanceKey = 0, long timestampMs = -1L)
		{
		}

		public override void MarkerPoint(int markerId, string name, int instanceKey = 0, long timestampMs = -1L)
		{
		}

		public unsafe override void MarkerPoint(int markerId, string name, OVRPlugin.Qpl.Annotation* annotations, int annotationCount, int instanceKey = 0, long timestampMs = -1L)
		{
		}

		public override void MarkerAnnotation(int markerId, string key, OVRPlugin.Qpl.Variant value, int instanceKey = 0)
		{
		}

		public override void MarkerAnnotation(int markerId, string annotationKey, string annotationValue, int instanceKey = 0)
		{
		}

		public override void MarkerEnd(int markerId, OVRPlugin.Qpl.ResultType resultTypeId = OVRPlugin.Qpl.ResultType.Success, int instanceKey = 0, long timestampMs = -1L)
		{
		}

		public override bool CreateMarkerHandle(string name, out int nameHandle)
		{
			nameHandle = 0;
			return false;
		}

		public override bool DestroyMarkerHandle(int nameHandle)
		{
			return false;
		}
	}

	private class QPLTelemetryClient : TelemetryClient
	{
		public override void MarkerStart(int markerId, int instanceKey = 0, long timestampMs = -1L, string joinId = null)
		{
			if (string.IsNullOrEmpty(joinId))
			{
				OVRPlugin.Qpl.MarkerStart(markerId, instanceKey, timestampMs);
			}
			else
			{
				OVRPlugin.Qpl.MarkerStartForJoin(markerId, joinId, OVRPlugin.Bool.False, instanceKey, timestampMs);
			}
		}

		public override void MarkerPointCached(int markerId, int nameHandle, int instanceKey = 0, long timestampMs = -1L)
		{
			OVRPlugin.Qpl.MarkerPointCached(markerId, nameHandle, instanceKey, timestampMs);
		}

		public override void MarkerPoint(int markerId, string name, int instanceKey = 0, long timestampMs = -1L)
		{
			OVRPlugin.Qpl.MarkerPoint(markerId, name, instanceKey, timestampMs);
		}

		public unsafe override void MarkerPoint(int markerId, string name, OVRPlugin.Qpl.Annotation* annotations, int annotationCount, int instanceKey = 0, long timestampMs = -1L)
		{
			OVRPlugin.Qpl.MarkerPoint(markerId, name, annotations, annotationCount, instanceKey, timestampMs);
		}

		public override void MarkerAnnotation(int markerId, string annotationKey, string annotationValue, int instanceKey = 0)
		{
			OVRPlugin.Qpl.MarkerAnnotation(markerId, annotationKey, annotationValue, instanceKey);
		}

		public override void MarkerAnnotation(int markerId, string key, OVRPlugin.Qpl.Variant value, int instanceKey = 0)
		{
			OVRPlugin.Qpl.MarkerAnnotation(markerId, key, value, instanceKey);
		}

		public override void MarkerEnd(int markerId, OVRPlugin.Qpl.ResultType resultTypeId = OVRPlugin.Qpl.ResultType.Success, int instanceKey = 0, long timestampMs = -1L)
		{
			OVRPlugin.Qpl.MarkerEnd(markerId, resultTypeId, instanceKey, timestampMs);
		}

		public override bool CreateMarkerHandle(string name, out int nameHandle)
		{
			return OVRPlugin.Qpl.CreateMarkerHandle(name, out nameHandle);
		}

		public override bool DestroyMarkerHandle(int nameHandle)
		{
			return OVRPlugin.Qpl.DestroyMarkerHandle(nameHandle);
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	internal class MarkersAttribute : Attribute
	{
	}

	private static readonly TelemetryClient InactiveClient = new NullTelemetryClient();

	public static readonly TelemetryClient ActiveClient = new QPLTelemetryClient();

	private static string _sdkVersionString;

	internal static bool IsActive => true;

	public static TelemetryClient Client
	{
		get
		{
			if (!IsActive)
			{
				return InactiveClient;
			}
			return ActiveClient;
		}
	}

	public static OVRTelemetryMarker Start(int markerId, int instanceKey = 0, long timestampMs = -1L)
	{
		return new OVRTelemetryMarker(markerId, instanceKey, timestampMs);
	}

	public static void SendEvent(int markerId, OVRPlugin.Qpl.ResultType result = OVRPlugin.Qpl.ResultType.Success)
	{
		Start(markerId, 0, -1L).SetResult(result).Send();
	}

	public static OVRTelemetryMarker AddSDKVersionAnnotation(this OVRTelemetryMarker marker)
	{
		if (string.IsNullOrEmpty(_sdkVersionString))
		{
			_sdkVersionString = OVRPlugin.version.ToString();
		}
		return marker.AddAnnotation("sdk_version", _sdkVersionString);
	}

	public static string GetPlayModeOrigin()
	{
		if (!Application.isPlaying)
		{
			return "Editor";
		}
		if (!Application.isEditor)
		{
			return "Build Play";
		}
		return "Editor Play";
	}

	public static OVRTelemetryMarker AddPlayModeOrigin(this OVRTelemetryMarker marker)
	{
		return marker.AddAnnotation("Origin", GetPlayModeOrigin());
	}

	public static string GetTelemetrySettingString(bool value)
	{
		if (!value)
		{
			return "disabled";
		}
		return "enabled";
	}
}
