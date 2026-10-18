using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public readonly struct OVRAnchor : IEquatable<OVRAnchor>, IDisposable
{
	[OVRResultStatus]
	public enum SaveResult
	{
		Success = 0,
		Failure = -1000,
		FailureInvalidAnchor = -1013,
		FailureDataIsInvalid = -1008,
		FailureInsufficientResources = -9000,
		FailureStorageAtCapacity = -9001,
		FailureInsufficientView = -9002,
		FailurePermissionInsufficient = -9003,
		FailureRateLimited = -9004,
		FailureTooDark = -9005,
		FailureTooBright = -9006,
		FailureUnsupported = -1004,
		FailurePersistenceNotEnabled = -2006
	}

	[OVRResultStatus]
	public enum EraseResult
	{
		Success = 0,
		Failure = -1000,
		FailureInvalidAnchor = -1013,
		FailureDataIsInvalid = -1008,
		FailureInsufficientResources = -9000,
		FailurePermissionInsufficient = -9003,
		FailureRateLimited = -9004,
		FailureUnsupported = -1004,
		FailurePersistenceNotEnabled = -2006
	}

	[OVRResultStatus]
	public enum FetchResult
	{
		Success = 0,
		Failure = -1000,
		FailureDataIsInvalid = -1008,
		FailureInvalidOption = -1001,
		FailureInsufficientResources = -9000,
		FailureInsufficientView = -9002,
		FailurePermissionInsufficient = -9003,
		FailureRateLimited = -9004,
		FailureTooDark = -9005,
		FailureTooBright = -9006,
		FailureUnsupported = -1004
	}

	[OVRResultStatus]
	public enum ShareResult
	{
		Success = 0,
		Failure = -1000,
		FailureOperationFailed = -1006,
		FailureInvalidParameter = -1001,
		FailureHandleInvalid = -1013,
		FailureDataIsInvalid = -1008,
		FailureNetworkTimeout = -2003,
		FailureNetworkRequestFailed = -2004,
		FailureMappingInsufficient = -2001,
		FailureLocalizationFailed = -2002,
		FailureSharableComponentNotEnabled = -2006,
		FailureCloudStorageDisabled = -2000,
		FailurePermissionInsufficient = -9003,
		FailureUnsupported = -1004
	}

	private struct FetchTaskData
	{
		public List<OVRAnchor> Anchors;

		public Action<List<OVRAnchor>, int> IncrementalResultsCallback;
	}

	private struct DeferredValue
	{
		public OVRTask<bool> Task;

		public bool EnabledDesired;

		public ulong RequestId;

		public double Timeout;

		public float StartTime;
	}

	private struct DeferredKey : IEquatable<DeferredKey>
	{
		public ulong Space;

		public OVRPlugin.SpaceComponentType ComponentType;

		public static DeferredKey FromEvent(OVRDeserialize.SpaceSetComponentStatusCompleteData eventData)
		{
			DeferredKey result = default(DeferredKey);
			result.Space = eventData.Space;
			result.ComponentType = eventData.ComponentType;
			return result;
		}

		public bool Equals(DeferredKey other)
		{
			if (Space == other.Space)
			{
				return ComponentType == other.ComponentType;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is DeferredKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			int num = Space.GetHashCode() * 486187739;
			int componentType = (int)ComponentType;
			return num + componentType.GetHashCode();
		}
	}

	public struct FetchOptions
	{
		public Guid? SingleUuid;

		public IEnumerable<Guid> Uuids;

		public Type SingleComponentType;

		public IEnumerable<Type> ComponentTypes;

		internal unsafe OVRPlugin.Result DiscoverSpaces(out ulong requestId)
		{
			OVRTelemetryMarker marker = OVRTelemetry.Start(163054959, 0, -1L);
			using OVRNativeList<FilterUnion> oVRNativeList2 = new OVRNativeList<FilterUnion>(Allocator.Temp);
			using OVRNativeList<IntPtr> oVRNativeList4 = new OVRNativeList<IntPtr>(Allocator.Temp);
			using OVRNativeList<long> oVRNativeList = OVRNativeList.WithSuggestedCapacityFrom(ComponentTypes).AllocateEmpty<long>(Allocator.Temp);
			if (SingleComponentType != null)
			{
				OVRPlugin.SpaceComponentType spaceComponentType = GetSpaceComponentType(SingleComponentType);
				oVRNativeList.Add((long)spaceComponentType);
				oVRNativeList2.Add(new FilterUnion
				{
					ComponentFilter = new OVRPlugin.SpaceDiscoveryFilterInfoComponents
					{
						Type = OVRPlugin.SpaceDiscoveryFilterType.Component,
						Component = spaceComponentType
					}
				});
			}
			foreach (Type item in ComponentTypes.ToNonAlloc())
			{
				OVRPlugin.SpaceComponentType spaceComponentType2 = GetSpaceComponentType(item);
				oVRNativeList.Add((long)spaceComponentType2);
				oVRNativeList2.Add(new FilterUnion
				{
					ComponentFilter = new OVRPlugin.SpaceDiscoveryFilterInfoComponents
					{
						Type = OVRPlugin.SpaceDiscoveryFilterType.Component,
						Component = spaceComponentType2
					}
				});
			}
			marker.AddAnnotation("component_types", oVRNativeList.Data, oVRNativeList.Count);
			using OVRNativeList<Guid> oVRNativeList3 = Uuids.ToNativeList(Allocator.Temp);
			if (SingleUuid.HasValue)
			{
				oVRNativeList3.Add(SingleUuid.Value);
			}
			if (SingleUuid.HasValue || Uuids != null)
			{
				oVRNativeList2.Add(new FilterUnion
				{
					IdFilter = new OVRPlugin.SpaceDiscoveryFilterInfoIds
					{
						Type = OVRPlugin.SpaceDiscoveryFilterType.Ids,
						Ids = oVRNativeList3.Data,
						NumIds = oVRNativeList3.Count
					}
				});
			}
			marker.AddAnnotation("uuid_count", oVRNativeList3.Count);
			for (int i = 0; i < oVRNativeList2.Count; i++)
			{
				oVRNativeList4.Add(new IntPtr(oVRNativeList2.PtrToElementAt(i)));
			}
			marker.AddAnnotation("total_filter_count", oVRNativeList4.Count);
			OVRPlugin.SpaceDiscoveryInfo info = default(OVRPlugin.SpaceDiscoveryInfo);
			info.NumFilters = (uint)oVRNativeList4.Count;
			info.Filters = (OVRPlugin.SpaceDiscoveryFilterInfoHeader**)oVRNativeList4.Data;
			OVRPlugin.Result result = OVRPlugin.DiscoverSpaces(in info, out requestId);
			Telemetry.SetSyncResult(marker, requestId, result);
			return result;
		}

		private static OVRPlugin.SpaceComponentType GetSpaceComponentType(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (!_typeMap.TryGetValue(type, out var value))
			{
				throw new ArgumentException(type.FullName + " is not a supported anchor component type (IOVRAnchorComponent).", "type");
			}
			return value;
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	internal struct FilterUnion
	{
		[FieldOffset(0)]
		public OVRPlugin.SpaceDiscoveryFilterType Type;

		[FieldOffset(0)]
		public OVRPlugin.SpaceDiscoveryFilterInfoComponents ComponentFilter;

		[FieldOffset(0)]
		public OVRPlugin.SpaceDiscoveryFilterInfoIds IdFilter;
	}

	public enum TrackableType
	{
		None,
		Keyboard,
		QRCode
	}

	private static class Telemetry
	{
		private readonly struct Key : IEquatable<Key>
		{
			private readonly int _markerId;

			private readonly ulong _requestId;

			public Key(MarkerId markerId, ulong requestId)
			{
				_markerId = (int)markerId;
				_requestId = requestId;
			}

			public Key(OVRTelemetryMarker marker, ulong requestId)
			{
				int markerId = marker.MarkerId;
				_markerId = markerId;
				_requestId = requestId;
			}

			public bool Equals(Key other)
			{
				if (_markerId == other._markerId)
				{
					return _requestId == other._requestId;
				}
				return false;
			}

			public override bool Equals(object obj)
			{
				if (obj is Key other)
				{
					return Equals(other);
				}
				return false;
			}

			public override int GetHashCode()
			{
				return _markerId.GetHashCode() * 486187739 + _requestId.GetHashCode();
			}
		}

		internal enum MarkerId
		{
			DiscoverSpaces = 163054959,
			SaveSpaces = 163056974,
			EraseSpaces = 163061838,
			QuerySpaces = 163069062,
			SaveSpaceList = 163065048,
			EraseSingleSpace = 163062284,
			ConfigureTracker = 163068237
		}

		internal static class Annotation
		{
			public const string ComponentTypes = "component_types";

			public const string UuidCount = "uuid_count";

			public const string SpaceCount = "space_count";

			public const string TotalFilterCount = "total_filter_count";

			public const string ResultsCount = "results_count";

			public const string SynchronousResult = "sync_result";

			public const string AsynchronousResult = "async_result";

			public const string StorageLocation = "storage_location";

			public const string Timeout = "timeout";

			public const string MaxResults = "max_results";

			public const string GroupCount = "group_count";

			public const string DynamicObjectClasses = "dynamic_object_classes";

			public const string MarkerTypes = "marker_types";
		}

		private static Dictionary<Key, OVRTelemetryMarker> s_markers = new Dictionary<Key, OVRTelemetryMarker>();

		public static void OnInit()
		{
			s_markers.Clear();
		}

		public static void AddMarker(ulong requestId, OVRTelemetryMarker marker)
		{
			s_markers.Add(new Key(marker, requestId), marker);
		}

		public static OVRTelemetryMarker Start(MarkerId markerId, ulong requestId, OVRPlugin.Result result)
		{
			OVRTelemetryMarker oVRTelemetryMarker = OVRTelemetry.Start((int)markerId, 0, -1L);
			SetSyncResult(oVRTelemetryMarker, requestId, result);
			return oVRTelemetryMarker;
		}

		public static void SetSyncResult(OVRTelemetryMarker marker, ulong requestId, OVRPlugin.Result result)
		{
			marker.AddAnnotation("sync_result", (long)result);
			if (result.IsSuccess())
			{
				if (requestId == 0L)
				{
					throw new ArgumentException("requestId must not be zero if the OVRPlugin method returns a successful result.", "requestId");
				}
				s_markers.Add(new Key(marker, requestId), marker);
			}
			else
			{
				marker.SetResult(OVRPlugin.Qpl.ResultType.Fail).Send();
			}
		}

		public static void SetAsyncResultAndSend(MarkerId markerId, ulong requestId, long result)
		{
			SetAsyncResult(markerId, requestId, result)?.Send();
		}

		public static OVRTelemetryMarker? SetAsyncResult(MarkerId markerId, ulong requestId, long result)
		{
			if (!s_markers.Remove(new Key(markerId, requestId), out var value))
			{
				return null;
			}
			return value.AddAnnotation("async_result", result).SetResult((result >= 0) ? OVRPlugin.Qpl.ResultType.Success : OVRPlugin.Qpl.ResultType.Fail);
		}

		public static OVRTelemetryMarker? GetMarker(MarkerId markerId, ulong requestId)
		{
			if (!TryGetMarker(markerId, requestId, out var marker))
			{
				return null;
			}
			return marker;
		}

		public static bool TryGetMarker(MarkerId markerId, ulong requestId, out OVRTelemetryMarker marker)
		{
			return s_markers.TryGetValue(new Key(markerId, requestId), out marker);
		}

		public static bool Remove(MarkerId markerId, ulong requestId, out OVRTelemetryMarker marker)
		{
			return s_markers.Remove(new Key(markerId, requestId), out marker);
		}

		public static OVRTelemetryMarker? GetRemove(MarkerId markerId, ulong requestId)
		{
			if (!Remove(markerId, requestId, out var marker))
			{
				return null;
			}
			return marker;
		}
	}

	[Serializable]
	public struct TrackerConfiguration : IEquatable<TrackerConfiguration>
	{
		[field: SerializeField]
		[field: Tooltip("When enabled, attempts to track physical keyboards in the environment.")]
		public bool KeyboardTrackingEnabled { get; set; }

		public static bool KeyboardTrackingSupported
		{
			get
			{
				bool value;
				return OVRPlugin.GetDynamicObjectTrackerSupported(out value).IsSuccess() && value && OVRPlugin.GetDynamicObjectKeyboardSupported(out value).IsSuccess() && value;
			}
		}

		internal bool RequiresDynamicObjectTracker => KeyboardTrackingEnabled;

		[field: SerializeField]
		[field: Tooltip("When enabled, attempts to track QR Codes in the environment.")]
		public bool QRCodeTrackingEnabled { get; set; }

		public static bool QRCodeTrackingSupported
		{
			get
			{
				bool markerTrackingSupported;
				return OVRPlugin.GetMarkerTrackingSupported(out markerTrackingSupported).IsSuccess() && markerTrackingSupported;
			}
		}

		internal bool RequiresMarkerTracker => QRCodeTrackingEnabled;

		internal OVRNativeList<OVRPlugin.DynamicObjectClass> ToDynamicObjectClasses(Allocator allocator)
		{
			OVRNativeList<OVRPlugin.DynamicObjectClass> result = new OVRNativeList<OVRPlugin.DynamicObjectClass>(allocator);
			if (KeyboardTrackingEnabled)
			{
				result.Add(OVRPlugin.DynamicObjectClass.Keyboard);
			}
			return result;
		}

		internal void ResetDynamicObjects()
		{
			TrackerConfiguration other = default(TrackerConfiguration);
			SetDynamicObjectState(in other);
		}

		internal void SetDynamicObjectState(in TrackerConfiguration other)
		{
			KeyboardTrackingEnabled = other.KeyboardTrackingEnabled;
		}

		internal OVRNativeList<OVRPlugin.MarkerType> ToMarkerTypes(Allocator allocator)
		{
			OVRNativeList<OVRPlugin.MarkerType> result = new OVRNativeList<OVRPlugin.MarkerType>(allocator);
			if (QRCodeTrackingEnabled)
			{
				result.Add(OVRPlugin.MarkerType.QRCode);
			}
			return result;
		}

		internal void ResetMarkers()
		{
			TrackerConfiguration other = default(TrackerConfiguration);
			SetMarkerState(in other);
		}

		internal void SetMarkerState(in TrackerConfiguration other)
		{
			QRCodeTrackingEnabled = other.QRCodeTrackingEnabled;
		}

		public void GetTrackableTypes(List<TrackableType> trackableTypes)
		{
			if (trackableTypes == null)
			{
				throw new ArgumentNullException("trackableTypes");
			}
			trackableTypes.Clear();
			if (KeyboardTrackingEnabled)
			{
				trackableTypes.Add(TrackableType.Keyboard);
			}
			if (QRCodeTrackingEnabled)
			{
				trackableTypes.Add(TrackableType.QRCode);
			}
		}

		public override string ToString()
		{
			List<string> list;
			using (new OVRObjectPool.ListScope<string>(out list))
			{
				list.Add(string.Format("{0}={1}", "KeyboardTrackingEnabled", KeyboardTrackingEnabled));
				list.Add(string.Format("{0}={1}", "QRCodeTrackingEnabled", QRCodeTrackingEnabled));
				return "TrackerConfiguration<" + string.Join(", ", list) + ">";
			}
		}

		public bool Equals(TrackerConfiguration other)
		{
			if (KeyboardTrackingEnabled != other.KeyboardTrackingEnabled)
			{
				return false;
			}
			if (QRCodeTrackingEnabled != other.QRCodeTrackingEnabled)
			{
				return false;
			}
			return true;
		}

		public override bool Equals(object obj)
		{
			if (obj is TrackerConfiguration other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(HashCode.Combine(0, KeyboardTrackingEnabled), QRCodeTrackingEnabled);
		}

		public static bool operator ==(TrackerConfiguration lhs, TrackerConfiguration rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(TrackerConfiguration lhs, TrackerConfiguration rhs)
		{
			return !lhs.Equals(rhs);
		}
	}

	[OVRResultStatus]
	public enum ConfigureTrackerResult
	{
		Success = 0,
		Failure = -1000,
		Invalid = -1008,
		NotSupported = -1004
	}

	public sealed class Tracker : IDisposable
	{
		private struct AsyncLock : IDisposable
		{
			private Tracker _tracker;

			private AsyncLock(Tracker tracker)
			{
				_tracker = tracker;
				_tracker._asyncOperationCount++;
			}

			public void Dispose()
			{
				_tracker._asyncOperationCount--;
			}

			public static async OVRTask<AsyncLock> AcquireAsync(Tracker tracker)
			{
				while (tracker._asyncOperationCount > 0)
				{
					await Task.Yield();
				}
				return new AsyncLock(tracker);
			}
		}

		private TrackerConfiguration _configuration;

		private int _asyncOperationCount;

		private ulong _markerTracker;

		private ulong _dynamicObjectTracker;

		public TrackerConfiguration Configuration => _configuration;

		private async OVRTask<OVRPlugin.Result> SetupMarkerTracker(TrackerConfiguration config)
		{
			if (config.QRCodeTrackingEnabled == _configuration.QRCodeTrackingEnabled)
			{
				return OVRPlugin.Result.Success;
			}
			if (_markerTracker != 0L)
			{
				OVRPlugin.DestroyMarkerTracker(_markerTracker);
				_markerTracker = 0uL;
			}
			_configuration.ResetMarkers();
			if (config.RequiresMarkerTracker)
			{
				OVRResult<ulong, OVRPlugin.Result> oVRResult = await CreateTrackerAsync(config);
				if (oVRResult.Success)
				{
					_markerTracker = oVRResult.Value;
					_configuration.SetMarkerState(in config);
				}
				return oVRResult.Status;
			}
			return OVRPlugin.Result.Success;
			static async OVRTask<OVRResult<ulong, OVRPlugin.Result>> CreateTrackerAsync(TrackerConfiguration config)
			{
				ulong future;
				OVRPlugin.Result result;
				using (OVRNativeList<OVRPlugin.MarkerType> oVRNativeList = config.ToMarkerTypes(Allocator.Temp))
				{
					result = OVRPlugin.CreateMarkerTrackerAsync(oVRNativeList, out future);
				}
				if (!result.IsSuccess())
				{
					return result;
				}
				result = await OVRFuture.When(future);
				if (!result.IsSuccess())
				{
					return result;
				}
				result = OVRPlugin.CreateMarkerTrackerComplete(future, out var completion);
				if (!result.IsSuccess())
				{
					return result;
				}
				return OVRResult<ulong, OVRPlugin.Result>.From(completion.MarkerTracker, completion.FutureResult);
			}
		}

		private async OVRTask<OVRPlugin.Result> SetupDynamicObjectTracker(TrackerConfiguration config)
		{
			_configuration.ResetDynamicObjects();
			if (config.RequiresDynamicObjectTracker)
			{
				OVRResult<ulong, OVRPlugin.Result> oVRResult = await CreateAndConfigureTrackerAsync(_dynamicObjectTracker, config);
				if (oVRResult.Success)
				{
					_dynamicObjectTracker = oVRResult.Value;
					_configuration.SetDynamicObjectState(in config);
				}
				else
				{
					_dynamicObjectTracker = 0uL;
				}
				return oVRResult.Status;
			}
			if (_dynamicObjectTracker != 0L)
			{
				OVRPlugin.DestroyDynamicObjectTracker(_dynamicObjectTracker);
				_dynamicObjectTracker = 0uL;
			}
			return OVRPlugin.Result.Success;
			static async OVRTask<OVRResult<ulong, OVRPlugin.Result>> CreateAndConfigureTrackerAsync(ulong tracker, TrackerConfiguration config)
			{
				if (tracker != 0L)
				{
					ulong value = tracker;
					return OVRResult<ulong, OVRPlugin.Result>.From(value, (await SetClassesAsync(tracker, config)).Status);
				}
				OVRResult<ulong, OVRPlugin.Result> oVRResult2 = await OVRPlugin.CreateDynamicObjectTrackerAsync();
				if (!oVRResult2.Success)
				{
					return oVRResult2.Status;
				}
				tracker = oVRResult2.Value;
				OVRResult<OVRPlugin.Result> oVRResult3 = await SetClassesAsync(tracker, config);
				if (!oVRResult3.Success)
				{
					OVRPlugin.DestroyDynamicObjectTracker(tracker);
					tracker = 0uL;
				}
				return OVRResult<ulong, OVRPlugin.Result>.From(tracker, oVRResult3.Status);
			}
			static OVRTask<OVRResult<OVRPlugin.Result>> SetClassesAsync(ulong tracker, TrackerConfiguration config)
			{
				using OVRNativeList<OVRPlugin.DynamicObjectClass> oVRNativeList = config.ToDynamicObjectClasses(Allocator.Temp);
				return OVRPlugin.SetDynamicObjectTrackedClassesAsync(tracker, oVRNativeList);
			}
		}

		public async OVRTask<OVRResult<ConfigureTrackerResult>> ConfigureAsync(TrackerConfiguration configuration)
		{
			using (await AsyncLock.AcquireAsync(this))
			{
				List<OVRTask<OVRPlugin.Result>> tasks;
				List<OVRPlugin.Result> results;
				using (new OVRObjectPool.TaskScope<OVRPlugin.Result>(out tasks, out results))
				{
					tasks.Add(SetupDynamicObjectTracker(configuration));
					tasks.Add(SetupMarkerTracker(configuration));
					await OVRTask.WhenAll(tasks, results);
					foreach (OVRPlugin.Result item in results)
					{
						if (!item.IsSuccess())
						{
							Debug.LogError($"Error while setting trackable configuration {configuration}: {item}");
						}
					}
					foreach (OVRPlugin.Result item2 in results)
					{
						if (!item2.IsSuccess())
						{
							return OVRResult.From((ConfigureTrackerResult)item2);
						}
					}
					return OVRResult.From(ConfigureTrackerResult.Success);
				}
			}
		}

		public OVRTask<OVRResult<List<OVRAnchor>, FetchResult>> FetchTrackablesAsync(List<OVRAnchor> anchors, Action<List<OVRAnchor>, int> incrementalResultsCallback = null)
		{
			if (anchors == null)
			{
				throw new ArgumentNullException("anchors");
			}
			List<TrackableType> list;
			using (new OVRObjectPool.ListScope<TrackableType>(out list))
			{
				_configuration.GetTrackableTypes(list);
				return OVRAnchor.FetchTrackablesAsync(anchors, (IEnumerable<TrackableType>)list, incrementalResultsCallback);
			}
		}

		~Tracker()
		{
			if (_markerTracker != 0L || _dynamicObjectTracker != 0L)
			{
				Debug.LogError("Tracker was not disposed of while one or more trackers were active, which leaks resources. Call Dispose() when no longer needed.");
			}
		}

		public async void Dispose()
		{
			using (await AsyncLock.AcquireAsync(this))
			{
				if (_dynamicObjectTracker != 0L)
				{
					OVRPlugin.DestroyDynamicObjectTracker(_dynamicObjectTracker);
				}
				_dynamicObjectTracker = 0uL;
				if (_markerTracker != 0L)
				{
					OVRPlugin.DestroyMarkerTracker(_markerTracker);
				}
				_markerTracker = 0uL;
				_configuration = default(TrackerConfiguration);
			}
		}
	}

	public static readonly OVRAnchor Null = new OVRAnchor(0uL, Guid.Empty);

	private static readonly Dictionary<DeferredKey, List<DeferredValue>> _deferredTasks = new Dictionary<DeferredKey, List<DeferredValue>>();

	internal static readonly Dictionary<Type, OVRPlugin.SpaceComponentType> _typeMap = new Dictionary<Type, OVRPlugin.SpaceComponentType>
	{
		{
			typeof(OVRLocatable),
			OVRPlugin.SpaceComponentType.Locatable
		},
		{
			typeof(OVRStorable),
			OVRPlugin.SpaceComponentType.Storable
		},
		{
			typeof(OVRSharable),
			OVRPlugin.SpaceComponentType.Sharable
		},
		{
			typeof(OVRBounded2D),
			OVRPlugin.SpaceComponentType.Bounded2D
		},
		{
			typeof(OVRBounded3D),
			OVRPlugin.SpaceComponentType.Bounded3D
		},
		{
			typeof(OVRSemanticLabels),
			OVRPlugin.SpaceComponentType.SemanticLabels
		},
		{
			typeof(OVRRoomLayout),
			OVRPlugin.SpaceComponentType.RoomLayout
		},
		{
			typeof(OVRAnchorContainer),
			OVRPlugin.SpaceComponentType.SpaceContainer
		},
		{
			typeof(OVRTriangleMesh),
			OVRPlugin.SpaceComponentType.TriangleMesh
		},
		{
			typeof(OVRDynamicObject),
			OVRPlugin.SpaceComponentType.DynamicObject
		},
		{
			typeof(OVRMarkerPayload),
			OVRPlugin.SpaceComponentType.MarkerPayload
		}
	};

	internal ulong Handle { get; }

	public Guid Uuid { get; }

	internal static void OnSpaceDiscoveryComplete(OVRDeserialize.SpaceDiscoveryCompleteData data)
	{
		if (OVRTask.TryGetPendingTask(data.RequestId, out OVRTask<OVRResult<List<OVRAnchor>, FetchResult>> task))
		{
			OVRResult<List<OVRAnchor>, FetchResult> result;
			if (task.TryGetInternalData<FetchTaskData>(out var data2))
			{
				Telemetry.GetMarker(Telemetry.MarkerId.DiscoverSpaces, data.RequestId)?.AddAnnotation("results_count", data2.Anchors?.Count ?? 0);
				result = OVRResult.From(data2.Anchors, (FetchResult)data.Result);
			}
			else
			{
				Debug.LogError("SpaceDiscovery completed but its task does not have an associated anchor List. " + $"RequestId={data.RequestId}, Result={data.Result}");
				result = OVRResult.From<List<OVRAnchor>, FetchResult>(null, (FetchResult)data.Result);
			}
			Telemetry.SetAsyncResultAndSend(Telemetry.MarkerId.DiscoverSpaces, data.RequestId, data.Result);
			task.SetResult(result);
		}
	}

	internal unsafe static void OnSpaceDiscoveryResultsAvailable(OVRDeserialize.SpaceDiscoveryResultsData data)
	{
		ulong requestId = data.RequestId;
		if (!OVRTask.TryGetPendingTask(requestId, out OVRTask<OVRResult<List<OVRAnchor>, FetchResult>> task) || !task.TryGetInternalData<FetchTaskData>(out var data2))
		{
			return;
		}
		NativeArray<OVRPlugin.SpaceDiscoveryResult> nativeArray = default(NativeArray<OVRPlugin.SpaceDiscoveryResult>);
		OVRPlugin.Result result = OVRPlugin.RetrieveSpaceDiscoveryResults(requestId, null, 0, out var countOutput);
		if (!result.IsSuccess())
		{
			return;
		}
		do
		{
			if (nativeArray.IsCreated)
			{
				nativeArray.Dispose();
			}
			nativeArray = new NativeArray<OVRPlugin.SpaceDiscoveryResult>(countOutput, Allocator.Temp);
			result = OVRPlugin.RetrieveSpaceDiscoveryResults(requestId, (OVRPlugin.SpaceDiscoveryResult*)nativeArray.GetUnsafePtr(), nativeArray.Length, out countOutput);
		}
		while (result == OVRPlugin.Result.Failure_InsufficientSize);
		int count = data2.Anchors.Count;
		using (nativeArray)
		{
			if (!result.IsSuccess() || countOutput == 0)
			{
				return;
			}
			for (int i = 0; i < countOutput; i++)
			{
				OVRPlugin.SpaceDiscoveryResult spaceDiscoveryResult = nativeArray[i];
				data2.Anchors.Add(new OVRAnchor(spaceDiscoveryResult.Space, spaceDiscoveryResult.Uuid));
			}
		}
		data2.IncrementalResultsCallback?.Invoke(data2.Anchors, count);
	}

	public static OVRTask<OVRResult<List<OVRAnchor>, FetchResult>> FetchAnchorsAsync(List<OVRAnchor> anchors, FetchOptions options, Action<List<OVRAnchor>, int> incrementalResultsCallback = null)
	{
		if (anchors == null)
		{
			throw new ArgumentNullException("anchors");
		}
		anchors.Clear();
		ulong requestId;
		return OVRTask.Build(options.DiscoverSpaces(out requestId), requestId).ToTask<List<OVRAnchor>, FetchResult>().WithInternalData(new FetchTaskData
		{
			Anchors = anchors,
			IncrementalResultsCallback = incrementalResultsCallback
		});
	}

	public static async OVRTask<OVRResult<List<OVRAnchor>, FetchResult>> FetchSharedAnchorsAsync(Guid groupUuid, List<OVRAnchor> anchors)
	{
		if (anchors == null)
		{
			throw new ArgumentNullException("anchors");
		}
		OVRPlugin.SpaceQueryInfo2 queryInfo = OVRSpaceQuery.ForGroupThrow(groupUuid, "groupUuid");
		return OVRResult.From(anchors, (FetchResult)(await FetchAnchors(anchors, queryInfo)));
	}

	public static async OVRTask<OVRResult<List<OVRAnchor>, FetchResult>> FetchSharedAnchorsAsync(Guid groupUuid, IEnumerable<Guid> allowedAnchorUuids, List<OVRAnchor> anchors)
	{
		if (allowedAnchorUuids == null)
		{
			throw new ArgumentNullException("allowedAnchorUuids");
		}
		if (anchors == null)
		{
			throw new ArgumentNullException("anchors");
		}
		OVRPlugin.SpaceQueryInfo2 queryInfo = OVRSpaceQuery.ForGroupThrow(groupUuid, "groupUuid", allowedAnchorUuids);
		return OVRResult.From(anchors, (FetchResult)(await FetchAnchors(anchors, queryInfo)));
	}

	public static OVRTask<OVRAnchor> CreateSpatialAnchorAsync(Pose trackingSpacePose)
	{
		OVRPlugin.SpatialAnchorCreateInfo createInfo = default(OVRPlugin.SpatialAnchorCreateInfo);
		createInfo.BaseTracking = OVRPlugin.GetTrackingOriginType();
		createInfo.PoseInSpace = new OVRPlugin.Posef
		{
			Orientation = trackingSpacePose.rotation.ToFlippedZQuatf(),
			Position = trackingSpacePose.position.ToFlippedZVector3f()
		};
		createInfo.Time = OVRPlugin.GetTimeInSeconds();
		ulong requestId;
		return OVRTask.Build(OVRPlugin.CreateSpatialAnchor(createInfo, out requestId), requestId).ToTask(Null);
	}

	public static OVRTask<OVRAnchor> CreateSpatialAnchorAsync(Transform transform, Camera centerEyeCamera)
	{
		if (transform == null)
		{
			throw new ArgumentNullException("transform");
		}
		if (centerEyeCamera == null)
		{
			throw new ArgumentNullException("centerEyeCamera");
		}
		OVRPose oVRPose = transform.ToTrackingSpacePose(centerEyeCamera);
		Pose trackingSpacePose = default(Pose);
		trackingSpacePose.position = oVRPose.position;
		trackingSpacePose.rotation = oVRPose.orientation;
		return CreateSpatialAnchorAsync(trackingSpacePose);
	}

	public unsafe OVRTask<OVRResult<SaveResult>> SaveAsync()
	{
		ulong handle = Handle;
		return SaveSpacesAsync(new ReadOnlySpan<ulong>(&handle, 1));
	}

	public static OVRTask<OVRResult<SaveResult>> SaveAsync(IEnumerable<OVRAnchor> anchors)
	{
		using OVRNativeList<ulong> oVRNativeList = OVRNativeList.WithSuggestedCapacityFrom(anchors).AllocateEmpty<ulong>(Allocator.Temp);
		foreach (OVRAnchor item in anchors.ToNonAlloc())
		{
			oVRNativeList.Add(item.Handle);
		}
		if (oVRNativeList.Count == 0)
		{
			return OVRTask.FromResult(OVRResult.From(SaveResult.Success));
		}
		return SaveSpacesAsync(oVRNativeList);
	}

	internal unsafe static OVRTask<OVRResult<SaveResult>> SaveSpacesAsync(ReadOnlySpan<ulong> spaces)
	{
		OVRTelemetryMarker marker = OVRTelemetry.Start(163056974, 0, -1L).AddAnnotation("space_count", spaces.Length);
		fixed (ulong* spaces2 = spaces)
		{
			ulong requestId;
			OVRPlugin.Result result = OVRPlugin.SaveSpaces(spaces2, spaces.Length, out requestId);
			Telemetry.SetSyncResult(marker, requestId, result);
			return OVRTask.Build(result, requestId).ToResultTask<SaveResult>();
		}
	}

	internal static void OnSaveSpacesResult(OVRDeserialize.SpacesSaveResultData eventData)
	{
		Telemetry.SetAsyncResultAndSend(Telemetry.MarkerId.SaveSpaces, eventData.RequestId, (long)eventData.Result);
	}

	public unsafe OVRTask<OVRResult<EraseResult>> EraseAsync()
	{
		Guid uuid = Uuid;
		return EraseSpacesAsync(default(ReadOnlySpan<ulong>), new ReadOnlySpan<Guid>(&uuid, 1));
	}

	public static OVRTask<OVRResult<EraseResult>> EraseAsync(IEnumerable<OVRAnchor> anchors, IEnumerable<Guid> uuids)
	{
		if (anchors == null && uuids == null)
		{
			throw new ArgumentException("One of anchors or uuids must not be null.");
		}
		using OVRNativeList<ulong> oVRNativeList = OVRNativeList.WithSuggestedCapacityFrom(anchors).AllocateEmpty<ulong>(Allocator.Temp);
		foreach (OVRAnchor item in anchors.ToNonAlloc())
		{
			oVRNativeList.Add(item.Handle);
		}
		using OVRNativeList<Guid> oVRNativeList2 = uuids.ToNativeList(Allocator.Temp);
		if (oVRNativeList.Count == 0 && oVRNativeList2.Count == 0)
		{
			return OVRTask.FromResult(OVRResult.From(EraseResult.Success));
		}
		return EraseSpacesAsync(oVRNativeList, oVRNativeList2);
	}

	private unsafe static OVRTask<OVRResult<EraseResult>> EraseSpacesAsync(ReadOnlySpan<ulong> spaces, ReadOnlySpan<Guid> uuids)
	{
		OVRTelemetryMarker marker = OVRTelemetry.Start(163061838, 0, -1L).AddAnnotation("space_count", spaces.Length).AddAnnotation("uuid_count", uuids.Length);
		fixed (ulong* spaces2 = spaces)
		{
			fixed (Guid* uuids2 = uuids)
			{
				ulong requestId;
				OVRPlugin.Result result = OVRPlugin.EraseSpaces((uint)spaces.Length, spaces2, (uint)uuids.Length, uuids2, out requestId);
				Telemetry.SetSyncResult(marker, requestId, result);
				return OVRTask.Build(result, requestId).ToResultTask<EraseResult>();
			}
		}
	}

	internal static void OnEraseSpacesResult(OVRDeserialize.SpacesEraseResultData eventData)
	{
		Telemetry.SetAsyncResultAndSend(Telemetry.MarkerId.EraseSpaces, eventData.RequestId, (long)eventData.Result);
	}

	public unsafe OVRTask<OVRResult<ShareResult>> ShareAsync(IEnumerable<OVRSpaceUser> users)
	{
		if (users == null)
		{
			throw new ArgumentNullException("users");
		}
		using OVRNativeList<ulong> oVRNativeList = OVRNativeList.WithSuggestedCapacityFrom(users).AllocateEmpty<ulong>(Allocator.Temp);
		foreach (OVRSpaceUser item in users.ToNonAlloc())
		{
			oVRNativeList.Add(item._handle);
		}
		if (oVRNativeList.Count < 1)
		{
			throw new ArgumentException("users must contain at least one user.");
		}
		ulong handle = Handle;
		return ShareSpacesAsync(new ReadOnlySpan<ulong>(&handle, 1), oVRNativeList);
	}

	public static OVRTask<OVRResult<ShareResult>> ShareAsync(IEnumerable<OVRAnchor> anchors, IEnumerable<OVRSpaceUser> users)
	{
		if (anchors == null)
		{
			throw new ArgumentNullException("anchors");
		}
		if (users == null)
		{
			throw new ArgumentNullException("users");
		}
		using OVRNativeList<ulong> oVRNativeList = OVRNativeList.WithSuggestedCapacityFrom(anchors).AllocateEmpty<ulong>(Allocator.Temp);
		foreach (OVRAnchor item in anchors.ToNonAlloc())
		{
			oVRNativeList.Add(item.Handle);
		}
		using OVRNativeList<ulong> oVRNativeList2 = OVRNativeList.WithSuggestedCapacityFrom(users).AllocateEmpty<ulong>(Allocator.Temp);
		foreach (OVRSpaceUser item2 in users.ToNonAlloc())
		{
			oVRNativeList2.Add(item2._handle);
		}
		if (oVRNativeList2.Count < 1)
		{
			throw new ArgumentException("users must contain at least one user.");
		}
		if (oVRNativeList.Count == 0)
		{
			return OVRTask.FromResult(OVRResult.From(ShareResult.Success));
		}
		return ShareSpacesAsync(oVRNativeList, oVRNativeList2);
	}

	private unsafe static OVRTask<OVRResult<ShareResult>> ShareSpacesAsync(ReadOnlySpan<ulong> spaces, ReadOnlySpan<ulong> users)
	{
		fixed (ulong* spaces2 = spaces)
		{
			fixed (ulong* userHandles = users)
			{
				ulong requestId;
				return OVRTask.Build(OVRPlugin.ShareSpaces(spaces2, (uint)spaces.Length, userHandles, (uint)users.Length, out requestId), requestId).ToResultTask<ShareResult>();
			}
		}
	}

	public unsafe OVRTask<OVRResult<ShareResult>> ShareAsync(Guid groupUuid)
	{
		ulong handle = Handle;
		ReadOnlySpan<ulong> anchors = new ReadOnlySpan<ulong>(&handle, 1);
		ReadOnlySpan<Guid> groupUuids = new ReadOnlySpan<Guid>(&groupUuid, 1);
		return ShareAsyncInternal(anchors, groupUuids);
	}

	public unsafe static OVRTask<OVRResult<ShareResult>> ShareAsync(IEnumerable<OVRAnchor> anchors, Guid groupUuid)
	{
		if (anchors == null)
		{
			throw new ArgumentNullException("anchors");
		}
		OVREnumerable<OVRAnchor> oVREnumerable = anchors.ToNonAlloc();
		using OVRNativeList<ulong> oVRNativeList = new OVRNativeList<ulong>(oVREnumerable.Count, Allocator.Temp);
		foreach (OVRAnchor item in oVREnumerable)
		{
			oVRNativeList.Add(item.Handle);
		}
		return ShareAsyncInternal(groupUuids: new ReadOnlySpan<Guid>(&groupUuid, 1), anchors: oVRNativeList);
	}

	internal unsafe static OVRTask<OVRResult<ShareResult>> ShareAsyncInternal(ReadOnlySpan<ulong> anchors, ReadOnlySpan<Guid> groupUuids)
	{
		OVRPlugin.ShareSpacesInfo info = default(OVRPlugin.ShareSpacesInfo);
		info.RecipientType = OVRPlugin.ShareSpacesRecipientType.Group;
		fixed (ulong* spaces = anchors)
		{
			fixed (Guid* groupUuids2 = groupUuids)
			{
				info.Spaces = spaces;
				info.SpaceCount = (uint)anchors.Length;
				OVRPlugin.ShareSpacesGroupRecipientInfo shareSpacesGroupRecipientInfo = default(OVRPlugin.ShareSpacesGroupRecipientInfo);
				shareSpacesGroupRecipientInfo.GroupCount = (uint)groupUuids.Length;
				shareSpacesGroupRecipientInfo.GroupUuids = groupUuids2;
				OVRPlugin.ShareSpacesGroupRecipientInfo shareSpacesGroupRecipientInfo2 = shareSpacesGroupRecipientInfo;
				info.RecipientInfo = (OVRPlugin.ShareSpacesRecipientInfoBase*)(&shareSpacesGroupRecipientInfo2);
				ulong requestId;
				return OVRTask.Build(OVRPlugin.ShareSpaces(in info, out requestId), requestId).ToResultTask<ShareResult>();
			}
		}
	}

	internal static void OnShareAnchorsToGroupsComplete(ulong requestId, OVRPlugin.Result result)
	{
		OVRTask.SetResult(requestId, OVRResult.From((ShareResult)result));
	}

	internal OVRAnchor(ulong handle, Guid uuid)
	{
		Handle = handle;
		Uuid = uuid;
	}

	public T GetComponent<T>() where T : struct, IOVRAnchorComponent<T>
	{
		if (!TryGetComponent<T>(out var component))
		{
			throw new InvalidOperationException($"Anchor {Uuid} does not have component {typeof(T).Name}");
		}
		return component;
	}

	public bool TryGetComponent<T>(out T component) where T : struct, IOVRAnchorComponent<T>
	{
		component = default(T);
		if (!OVRPlugin.GetSpaceComponentStatusInternal(Handle, component.Type, out var _, out var _).IsSuccess())
		{
			return false;
		}
		component = component.FromAnchor(this);
		return true;
	}

	public bool SupportsComponent<T>() where T : struct, IOVRAnchorComponent<T>
	{
		bool enabled;
		bool changePending;
		return OVRPlugin.GetSpaceComponentStatusInternal(Handle, default(T).Type, out enabled, out changePending).IsSuccess();
	}

	public unsafe bool GetSupportedComponents(List<OVRPlugin.SpaceComponentType> components)
	{
		components.Clear();
		if (!OVRPlugin.EnumerateSpaceSupportedComponents(Handle, 0u, out var countOutput, null).IsSuccess())
		{
			return false;
		}
		OVRPlugin.SpaceComponentType* ptr = stackalloc OVRPlugin.SpaceComponentType[(int)countOutput];
		if (!OVRPlugin.EnumerateSpaceSupportedComponents(Handle, countOutput, out countOutput, ptr).IsSuccess())
		{
			return false;
		}
		for (uint num = 0u; num < countOutput; num++)
		{
			components.Add(ptr[num]);
		}
		return true;
	}

	public bool Equals(OVRAnchor other)
	{
		if (Handle.Equals(other.Handle))
		{
			return Uuid.Equals(other.Uuid);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is OVRAnchor other)
		{
			return Equals(other);
		}
		return false;
	}

	public static bool operator ==(OVRAnchor lhs, OVRAnchor rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(OVRAnchor lhs, OVRAnchor rhs)
	{
		return !lhs.Equals(rhs);
	}

	public override int GetHashCode()
	{
		return Handle.GetHashCode() * 486187739 + Uuid.GetHashCode();
	}

	public override string ToString()
	{
		return Uuid.ToString();
	}

	public void Dispose()
	{
		OVRPlugin.DestroySpace(Handle);
	}

	[RuntimeInitializeOnLoadMethod]
	internal static void Init()
	{
		_deferredTasks.Clear();
		Telemetry.OnInit();
	}

	internal unsafe static OVRTask<OVRPlugin.Result> FetchAnchors(IList<OVRAnchor> anchors, OVRPlugin.SpaceQueryInfo2 queryInfo)
	{
		if (anchors == null)
		{
			throw new ArgumentNullException("anchors");
		}
		anchors.Clear();
		OVRTelemetryMarker marker = OVRTelemetry.Start(163069062, 0, -1L).AddAnnotation("timeout", queryInfo.Timeout).AddAnnotation("max_results", queryInfo.MaxQuerySpaces)
			.AddAnnotation("storage_location", (long)queryInfo.Location);
		switch (queryInfo.FilterType)
		{
		case OVRPlugin.SpaceQueryFilterType.Components:
		{
			long* annotationValues = stackalloc long[1] { (long)queryInfo.ComponentsInfo.Components[0] };
			marker.AddAnnotation("component_types", annotationValues, queryInfo.ComponentsInfo.NumComponents);
			break;
		}
		case OVRPlugin.SpaceQueryFilterType.Group:
			marker.AddAnnotation("group_count", 1L);
			break;
		case OVRPlugin.SpaceQueryFilterType.Ids:
			marker.AddAnnotation("uuid_count", queryInfo.IdInfo.NumIds);
			break;
		}
		ulong requestId;
		OVRPlugin.Result result = OVRPlugin.QuerySpaces2(queryInfo, out requestId);
		Telemetry.SetSyncResult(marker, requestId, result);
		return OVRTask.Build(result, requestId).ToTask().WithInternalData(anchors);
	}

	internal static OVRTask<bool> CreateDeferredSpaceComponentStatusTask(ulong space, OVRPlugin.SpaceComponentType componentType, bool enabledDesired, double timeout)
	{
		DeferredKey deferredKey = default(DeferredKey);
		deferredKey.Space = space;
		deferredKey.ComponentType = componentType;
		DeferredKey key = deferredKey;
		if (!_deferredTasks.TryGetValue(key, out var value))
		{
			value = OVRObjectPool.List<DeferredValue>();
			_deferredTasks.Add(key, value);
		}
		OVRTask<bool> oVRTask = OVRTask.FromGuid<bool>(Guid.NewGuid());
		value.Add(new DeferredValue
		{
			EnabledDesired = enabledDesired,
			Task = oVRTask,
			Timeout = timeout,
			StartTime = Time.realtimeSinceStartup
		});
		return oVRTask;
	}

	internal static void OnSpaceSetComponentStatusComplete(OVRDeserialize.SpaceSetComponentStatusCompleteData eventData)
	{
		DeferredKey key = DeferredKey.FromEvent(eventData);
		if (!_deferredTasks.TryGetValue(key, out var value))
		{
			return;
		}
		try
		{
			bool flag = eventData.Enabled != 0;
			for (int i = 0; i < value.Count; i++)
			{
				DeferredValue value2 = value[i];
				OVRTask<bool> task = value2.Task;
				bool? flag2 = null;
				bool enabled;
				bool changePending;
				if (eventData.RequestId == value2.RequestId)
				{
					flag2 = eventData.Result >= 0;
				}
				else if (flag == value2.EnabledDesired)
				{
					flag2 = true;
				}
				else if (!OVRPlugin.GetSpaceComponentStatus(eventData.Space, eventData.ComponentType, out enabled, out changePending))
				{
					flag2 = false;
				}
				else if (!changePending)
				{
					double num = value2.Timeout;
					if (num > 0.0)
					{
						num -= (double)(Time.realtimeSinceStartup - value2.StartTime);
						if (num <= 0.0)
						{
							flag2 = false;
						}
					}
					if (!flag2.HasValue)
					{
						if (OVRPlugin.SetSpaceComponentStatus(eventData.Space, eventData.ComponentType, value2.EnabledDesired, num, out var requestId))
						{
							value2.RequestId = requestId;
							value[i] = value2;
						}
						else
						{
							flag2 = false;
						}
					}
				}
				if (flag2.HasValue)
				{
					value.RemoveAt(i--);
					task.SetResult(flag2.Value);
				}
			}
		}
		finally
		{
			if (value.Count == 0)
			{
				OVRObjectPool.Return(value);
				_deferredTasks.Remove(key);
			}
		}
	}

	[Obsolete("Use the overload of FetchAnchorsAsync that accepts a FetchOptions parameter")]
	public static OVRTask<bool> FetchAnchorsAsync<T>(IList<OVRAnchor> anchors, OVRSpace.StorageLocation location = OVRSpace.StorageLocation.Local, int maxResults = 1024, double timeout = 0.0) where T : struct, IOVRAnchorComponent<T>
	{
		if (anchors == null)
		{
			throw new ArgumentNullException("anchors");
		}
		return FetchAnchorsAsync(default(T).Type, anchors, location, maxResults, timeout);
	}

	[Obsolete("Use the overload of FetchAnchorsAsync that accepts a FetchOptions parameter")]
	public static OVRTask<bool> FetchAnchorsAsync(IEnumerable<Guid> uuids, IList<OVRAnchor> anchors, OVRSpace.StorageLocation location = OVRSpace.StorageLocation.Local, double timeout = 0.0)
	{
		if (uuids == null)
		{
			throw new ArgumentNullException("uuids");
		}
		if (anchors == null)
		{
			throw new ArgumentNullException("anchors");
		}
		return execute();
		async OVRTask<bool> execute()
		{
			OVRPlugin.SpaceQueryInfo2 queryInfo = OVRSpaceQuery.ForAnchorsUnchecked(uuids.ToNonAlloc());
			queryInfo.Location = location.ToSpaceStorageLocation();
			queryInfo.Timeout = timeout;
			return (await FetchAnchors(anchors, queryInfo)).IsSuccess();
		}
	}

	internal static void OnSpaceQueryComplete(OVRDeserialize.SpaceQueryCompleteData data)
	{
		OVRTelemetryMarker? oVRTelemetryMarker = null;
		OVRPlugin.Result? result = null;
		try
		{
			oVRTelemetryMarker = Telemetry.SetAsyncResult(Telemetry.MarkerId.QuerySpaces, data.RequestId, data.Result);
			ulong requestId = data.RequestId;
			if (!OVRTask.TryGetPendingTask(data.RequestId, out OVRTask<OVRPlugin.Result> task))
			{
				return;
			}
			result = (OVRPlugin.Result)data.Result;
			if (data.Result < 0)
			{
				return;
			}
			if (!task.TryGetInternalData<IList<OVRAnchor>>(out var data2) || data2 == null)
			{
				result = OVRPlugin.Result.Failure_DataIsInvalid;
				return;
			}
			if (!OVRPlugin.RetrieveSpaceQueryResults(requestId, out var results, Allocator.Temp))
			{
				result = OVRPlugin.Result.Failure_OperationFailed;
				return;
			}
			using (results)
			{
				oVRTelemetryMarker?.AddAnnotation("results_count", results.Length);
				foreach (OVRPlugin.SpaceQueryResult item in results)
				{
					data2.Add(new OVRAnchor(item.space, item.uuid));
				}
				result = (OVRPlugin.Result)data.Result;
			}
		}
		finally
		{
			oVRTelemetryMarker?.Send();
			if (result.HasValue)
			{
				OVRTask.SetResult(data.RequestId, result.Value);
			}
		}
	}

	[Obsolete]
	internal static async OVRTask<bool> FetchAnchorsAsync(OVRPlugin.SpaceComponentType type, IList<OVRAnchor> anchors, OVRSpace.StorageLocation location = OVRSpace.StorageLocation.Local, int maxResults = 1024, double timeout = 0.0)
	{
		OVRPlugin.SpaceQueryInfo2 queryInfo = OVRSpaceQuery.ForComponentUnchecked(type);
		queryInfo.Location = location.ToSpaceStorageLocation();
		queryInfo.MaxQuerySpaces = maxResults;
		queryInfo.Timeout = timeout;
		return (await FetchAnchors(anchors, queryInfo)).IsSuccess();
	}

	[Obsolete]
	internal unsafe static OVRPlugin.Result SaveSpaceList(ulong* spaces, uint numSpaces, OVRPlugin.SpaceStorageLocation location, out ulong requestId)
	{
		OVRTelemetryMarker marker = OVRTelemetry.Start(163065048, 0, -1L).AddAnnotation("space_count", numSpaces).AddAnnotation("storage_location", (long)location);
		OVRPlugin.Result result = OVRPlugin.SaveSpaceList(spaces, numSpaces, location, out requestId);
		Telemetry.SetSyncResult(marker, requestId, result);
		return result;
	}

	internal static void OnSpaceListSaveResult(OVRDeserialize.SpaceListSaveResultData eventData)
	{
		Telemetry.SetAsyncResultAndSend(Telemetry.MarkerId.SaveSpaceList, eventData.RequestId, eventData.Result);
	}

	[Obsolete]
	internal static OVRPlugin.Result EraseSpace(ulong space, OVRPlugin.SpaceStorageLocation location, out ulong requestId)
	{
		OVRTelemetryMarker marker = OVRTelemetry.Start(163062284, 0, -1L).AddAnnotation("storage_location", (long)location);
		OVRPlugin.Result result = OVRPlugin.EraseSpaceWithResult(space, location, out requestId);
		Telemetry.SetSyncResult(marker, requestId, result);
		return result;
	}

	internal static void OnSpaceEraseComplete(OVRDeserialize.SpaceEraseCompleteData eventData)
	{
		Telemetry.SetAsyncResultAndSend(Telemetry.MarkerId.EraseSingleSpace, eventData.RequestId, eventData.Result);
	}

	public TrackableType GetTrackableType()
	{
		List<OVRPlugin.SpaceComponentType> list;
		using (new OVRObjectPool.ListScope<OVRPlugin.SpaceComponentType>(out list))
		{
			if (!GetSupportedComponents(list))
			{
				return TrackableType.None;
			}
			using List<OVRPlugin.SpaceComponentType>.Enumerator enumerator = list.GetEnumerator();
			while (enumerator.MoveNext())
			{
				switch (enumerator.Current)
				{
				case OVRPlugin.SpaceComponentType.DynamicObject:
				{
					OVRDynamicObject component2 = GetComponent<OVRDynamicObject>();
					if (component2.IsEnabled)
					{
						return component2.TrackableType;
					}
					break;
				}
				case OVRPlugin.SpaceComponentType.MarkerPayload:
				{
					OVRMarkerPayload component = GetComponent<OVRMarkerPayload>();
					if (component.IsEnabled && component.PayloadType.IsQRCode())
					{
						return TrackableType.QRCode;
					}
					break;
				}
				}
			}
		}
		return TrackableType.None;
	}

	private static void GetRequiredComponents(IEnumerable<TrackableType> trackableTypes, HashSet<TrackableType> trackableTypesOut, HashSet<OVRPlugin.SpaceComponentType> requiredComponentsOut)
	{
		foreach (TrackableType item in trackableTypes.ToNonAlloc())
		{
			trackableTypesOut.Add(item);
			switch (item)
			{
			case TrackableType.Keyboard:
				requiredComponentsOut.Add(OVRPlugin.SpaceComponentType.DynamicObject);
				break;
			case TrackableType.QRCode:
				requiredComponentsOut.Add(OVRPlugin.SpaceComponentType.MarkerPayload);
				break;
			}
		}
	}

	public static async OVRTask<OVRResult<List<OVRAnchor>, FetchResult>> FetchTrackablesAsync(List<OVRAnchor> anchors, IEnumerable<TrackableType> trackableTypes, Action<List<OVRAnchor>, int> incrementalResultsCallback = null)
	{
		if (anchors == null)
		{
			throw new ArgumentNullException("anchors");
		}
		if (trackableTypes == null)
		{
			throw new ArgumentNullException("trackableTypes");
		}
		anchors.Clear();
		HashSet<TrackableType> set;
		using (new OVRObjectPool.HashSetScope<TrackableType>(out set))
		{
			HashSet<OVRPlugin.SpaceComponentType> set2;
			using (new OVRObjectPool.HashSetScope<OVRPlugin.SpaceComponentType>(out set2))
			{
				GetRequiredComponents(trackableTypes, set, set2);
				List<OVRTask<OVRPlugin.Result>> tasks;
				List<OVRPlugin.Result> results;
				using (new OVRObjectPool.TaskScope<OVRPlugin.Result>(out tasks, out results))
				{
					foreach (OVRPlugin.SpaceComponentType item in set2)
					{
						tasks.Add(QuerySingleComponentAsync(anchors, set, item, incrementalResultsCallback));
					}
					foreach (OVRPlugin.Result item2 in await OVRTask.WhenAll(tasks, results))
					{
						if (!item2.IsSuccess())
						{
							return OVRResult.From(anchors, (FetchResult)item2);
						}
					}
				}
				return OVRResult.From(anchors, FetchResult.Success);
			}
		}
		static bool DoesComponentMatchTrackableType(HashSet<TrackableType> trackableTypes, OVRAnchor anchor, OVRPlugin.SpaceComponentType componentType)
		{
			switch (componentType)
			{
			case OVRPlugin.SpaceComponentType.DynamicObject:
				return trackableTypes.Contains(anchor.GetComponent<OVRDynamicObject>().TrackableType);
			case OVRPlugin.SpaceComponentType.MarkerPayload:
				if (anchor.GetComponent<OVRMarkerPayload>().PayloadType.IsQRCode())
				{
					return trackableTypes.Contains(TrackableType.QRCode);
				}
				break;
			}
			return false;
		}
		static async OVRTask<OVRPlugin.Result> QuerySingleComponentAsync(List<OVRAnchor> anchors, HashSet<TrackableType> trackableTypes, OVRPlugin.SpaceComponentType componentType, Action<List<OVRAnchor>, int> incrementalResultsCallback)
		{
			List<OVRAnchor> anchorsWithComponent;
			using (new OVRObjectPool.ListScope<OVRAnchor>(out anchorsWithComponent))
			{
				OVRPlugin.SpaceQueryInfo2 queryInfo = OVRSpaceQuery.ForComponentUnchecked(componentType);
				queryInfo.Location = OVRPlugin.SpaceStorageLocation.Local;
				OVRPlugin.Result result = await FetchAnchors(anchorsWithComponent, queryInfo);
				if (!result.IsSuccess())
				{
					return result;
				}
				int count = anchors.Count;
				foreach (OVRAnchor item3 in anchorsWithComponent)
				{
					if (DoesComponentMatchTrackableType(trackableTypes, item3, componentType))
					{
						anchors.Add(item3);
					}
				}
				if (count != anchors.Count)
				{
					incrementalResultsCallback?.Invoke(anchors, count);
				}
				return result;
			}
		}
	}
}
