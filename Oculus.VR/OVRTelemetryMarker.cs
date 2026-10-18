using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

internal struct OVRTelemetryMarker : IDisposable
{
	internal struct OVRTelemetryMarkerState
	{
		public bool Sent { get; set; }

		public OVRPlugin.Qpl.ResultType Result { get; set; }

		public OVRTelemetryMarkerState(bool sent, OVRPlugin.Qpl.ResultType result)
		{
			Result = result;
			Sent = sent;
		}
	}

	private readonly OVRTelemetry.TelemetryClient _client;

	private static string _applicationIdentifier;

	private static string _unityVersion;

	private static bool? _isBatchMode;

	private const string TelemetryEnabledKey = "OVRTelemetry.TelemetryEnabled";

	private OVRTelemetryMarkerState State { get; set; }

	public bool Sent => State.Sent;

	public OVRPlugin.Qpl.ResultType Result => State.Result;

	public int MarkerId { get; }

	public int InstanceKey { get; }

	private static string ApplicationIdentifier => _applicationIdentifier ?? (_applicationIdentifier = Application.identifier);

	private static string UnityVersion => _unityVersion ?? (_unityVersion = Application.unityVersion);

	private static bool IsBatchMode
	{
		get
		{
			bool valueOrDefault = _isBatchMode.GetValueOrDefault();
			if (!_isBatchMode.HasValue)
			{
				valueOrDefault = Application.isBatchMode;
				_isBatchMode = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public OVRTelemetryMarker(int markerId, int instanceKey = 0, long timestampMs = -1L, string joindId = null)
		: this(OVRTelemetry.Client, markerId, instanceKey, timestampMs, joindId)
	{
	}

	internal OVRTelemetryMarker(OVRTelemetry.TelemetryClient client, int markerId, int instanceKey = 0, long timestampMs = -1L, string joinId = null)
	{
		MarkerId = markerId;
		InstanceKey = instanceKey;
		_client = client;
		State = new OVRTelemetryMarkerState(sent: false, OVRPlugin.Qpl.ResultType.Success);
		_client.MarkerStart(markerId, instanceKey, timestampMs, joinId);
	}

	public OVRTelemetryMarker SetResult(OVRPlugin.Qpl.ResultType result)
	{
		State = new OVRTelemetryMarkerState(Sent, result);
		return this;
	}

	public OVRTelemetryMarker AddAnnotation(string annotationKey, string annotationValue)
	{
		if (string.IsNullOrEmpty(annotationKey))
		{
			return this;
		}
		if (annotationValue == null)
		{
			annotationValue = string.Empty;
		}
		_client.MarkerAnnotation(MarkerId, annotationKey, annotationValue, InstanceKey);
		return this;
	}

	public OVRTelemetryMarker AddAnnotation(string annotationKey, bool annotationValue)
	{
		if (string.IsNullOrEmpty(annotationKey))
		{
			return this;
		}
		_client.MarkerAnnotation(MarkerId, annotationKey, annotationValue, InstanceKey);
		return this;
	}

	public OVRTelemetryMarker AddAnnotation(string annotationKey, double annotationValue)
	{
		if (string.IsNullOrEmpty(annotationKey))
		{
			return this;
		}
		_client.MarkerAnnotation(MarkerId, annotationKey, annotationValue, InstanceKey);
		return this;
	}

	public OVRTelemetryMarker AddAnnotation(string annotationKey, long annotationValue)
	{
		if (string.IsNullOrEmpty(annotationKey))
		{
			return this;
		}
		_client.MarkerAnnotation(MarkerId, annotationKey, annotationValue, InstanceKey);
		return this;
	}

	public unsafe OVRTelemetryMarker AddAnnotation(string annotationKey, byte** annotationValues, int count)
	{
		if (string.IsNullOrEmpty(annotationKey))
		{
			return this;
		}
		_client.MarkerAnnotation(MarkerId, annotationKey, annotationValues, count, InstanceKey);
		return this;
	}

	public unsafe OVRTelemetryMarker AddAnnotation(string annotationKey, ReadOnlySpan<long> annotationValues)
	{
		fixed (long* annotationValues2 = annotationValues)
		{
			return AddAnnotation(annotationKey, annotationValues2, annotationValues.Length);
		}
	}

	public unsafe OVRTelemetryMarker AddAnnotation(string annotationKey, long* annotationValues, int count)
	{
		if (string.IsNullOrEmpty(annotationKey))
		{
			return this;
		}
		_client.MarkerAnnotation(MarkerId, annotationKey, annotationValues, count, InstanceKey);
		return this;
	}

	public unsafe OVRTelemetryMarker AddAnnotation<T>(string annotationKey, ReadOnlySpan<T> annotationValues) where T : unmanaged, Enum
	{
		Type underlyingType = Enum.GetUnderlyingType(typeof(T));
		if (underlyingType == typeof(long) || underlyingType == typeof(ulong))
		{
			fixed (T* annotationValues2 = annotationValues)
			{
				return AddAnnotation(annotationKey, (long*)annotationValues2, annotationValues.Length);
			}
		}
		NativeArray<long> nativeArray = new NativeArray<long>(annotationValues.Length, Allocator.Temp);
		try
		{
			for (int i = 0; i < annotationValues.Length; i++)
			{
				nativeArray[i] = UnsafeUtility.EnumToInt(annotationValues[i]);
			}
			return AddAnnotation(annotationKey, (long*)nativeArray.GetUnsafeReadOnlyPtr(), nativeArray.Length);
		}
		finally
		{
			nativeArray.Dispose();
		}
	}

	public unsafe OVRTelemetryMarker AddAnnotation(string annotationKey, ReadOnlySpan<double> annotationValues)
	{
		fixed (double* annotationValues2 = annotationValues)
		{
			return AddAnnotation(annotationKey, annotationValues2, annotationValues.Length);
		}
	}

	public unsafe OVRTelemetryMarker AddAnnotation(string annotationKey, double* annotationValues, int count)
	{
		if (string.IsNullOrEmpty(annotationKey))
		{
			return this;
		}
		_client.MarkerAnnotation(MarkerId, annotationKey, annotationValues, count, InstanceKey);
		return this;
	}

	public unsafe OVRTelemetryMarker AddAnnotation(string annotationKey, ReadOnlySpan<OVRPlugin.Bool> annotationValues)
	{
		fixed (OVRPlugin.Bool* annotationValues2 = annotationValues)
		{
			return AddAnnotation(annotationKey, annotationValues2, annotationValues.Length);
		}
	}

	public unsafe OVRTelemetryMarker AddAnnotation(string annotationKey, OVRPlugin.Bool* annotationValues, int count)
	{
		if (string.IsNullOrEmpty(annotationKey))
		{
			return this;
		}
		_client.MarkerAnnotation(MarkerId, annotationKey, annotationValues, count, InstanceKey);
		return this;
	}

	public OVRTelemetryMarker AddAnnotationIfNotNullOrEmpty(string annotationKey, string annotationValue)
	{
		if (!string.IsNullOrEmpty(annotationValue))
		{
			return AddAnnotation(annotationKey, annotationValue);
		}
		return this;
	}

	private bool GetOVRTelemetryConsent()
	{
		return false;
	}

	public OVRTelemetryMarker Send()
	{
		AddAnnotation("ProjectName", ApplicationIdentifier);
		AddAnnotation("ProjectGuid", OVRRuntimeSettings.Instance.TelemetryProjectGuid);
		AddAnnotation("BatchMode", IsBatchMode);
		AddAnnotation("ProcessorType", SystemInfo.processorType);
		State = new OVRTelemetryMarkerState(sent: true, Result);
		_client.MarkerEnd(MarkerId, Result, InstanceKey, -1L);
		return this;
	}

	public OVRTelemetryMarker SendIf(bool condition)
	{
		if (condition)
		{
			return Send();
		}
		State = new OVRTelemetryMarkerState(sent: true, Result);
		return this;
	}

	public OVRTelemetryMarker AddPoint(OVRTelemetry.MarkerPoint point)
	{
		_client.MarkerPointCached(MarkerId, point.NameHandle, InstanceKey, -1L);
		return this;
	}

	public OVRTelemetryMarker AddPoint(string name)
	{
		_client.MarkerPoint(MarkerId, name, InstanceKey, -1L);
		return this;
	}

	public unsafe OVRTelemetryMarker AddPoint(string name, OVRPlugin.Qpl.Annotation.Builder annotationBuilder)
	{
		using NativeArray<OVRPlugin.Qpl.Annotation> nativeArray = annotationBuilder.ToNativeArray();
		return AddPoint(name, (OVRPlugin.Qpl.Annotation*)nativeArray.GetUnsafeReadOnlyPtr(), nativeArray.Length);
	}

	public unsafe OVRTelemetryMarker AddPoint(string name, OVRPlugin.Qpl.Annotation* annotations, int annotationCount)
	{
		_client.MarkerPoint(MarkerId, name, annotations, annotationCount, InstanceKey, -1L);
		return this;
	}

	public void Dispose()
	{
		if (!Sent)
		{
			Send();
		}
	}
}
