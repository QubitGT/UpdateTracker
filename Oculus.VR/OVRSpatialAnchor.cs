using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Meta.XR.Util;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

[DisallowMultipleComponent]
[HelpURL("https://developer.oculus.com/documentation/unity/unity-spatial-anchors-persist-content/#ovrspatialanchor-component")]
[Feature(Feature.Anchors)]
public class OVRSpatialAnchor : MonoBehaviour
{
	private struct MultiAnchorDelegatePair
	{
		public List<OVRSpatialAnchor> Anchors;

		public Action<ICollection<OVRSpatialAnchor>, OperationResult> Delegate;
	}

	public readonly struct UnboundAnchor
	{
		internal readonly OVRSpace _space;

		public Guid Uuid { get; }

		public bool Localized
		{
			get
			{
				bool enabled;
				bool changePending;
				return OVRPlugin.GetSpaceComponentStatus(_space, OVRPlugin.SpaceComponentType.Locatable, out enabled, out changePending) && enabled;
			}
		}

		public bool Localizing
		{
			get
			{
				bool enabled;
				bool changePending;
				return OVRPlugin.GetSpaceComponentStatus(_space, OVRPlugin.SpaceComponentType.Locatable, out enabled, out changePending) && !enabled && changePending;
			}
		}

		[Obsolete("Use TryGetPose instead.")]
		public Pose Pose
		{
			get
			{
				if (!TryGetPose(out var pose))
				{
					throw new InvalidOperationException($"[{Uuid}] Anchor must be localized before obtaining its pose.");
				}
				return pose;
			}
		}

		public bool TryGetPose(out Pose pose)
		{
			OVRAnchor oVRAnchor = new OVRAnchor(_space, Uuid);
			if (oVRAnchor == OVRAnchor.Null)
			{
				throw new InvalidOperationException("The UnboundAnchor is not valid. Was it default (zero) initialized?");
			}
			if (!oVRAnchor.TryGetComponent<OVRLocatable>(out var component))
			{
				throw new InvalidOperationException($"Anchor {Uuid} is not localizable.");
			}
			if (!component.IsEnabled)
			{
				throw new InvalidOperationException($"The anchor {Uuid} is not localized. An anchor must be localized before getting the pose.");
			}
			if (OVRSpatialAnchor.TryGetPose(_space, out OVRPose pose2))
			{
				pose = new Pose(pose2.position, pose2.orientation);
				return true;
			}
			pose = Pose.identity;
			return false;
		}

		public OVRTask<bool> LocalizeAsync(double timeout = 0.0)
		{
			OVRAnchor oVRAnchor = new OVRAnchor(_space, Uuid);
			if (oVRAnchor.TryGetComponent<OVRStorable>(out var component))
			{
				component.SetEnabledAsync(enabled: true);
			}
			if (oVRAnchor.TryGetComponent<OVRSharable>(out var component2))
			{
				component2.SetEnabledAsync(enabled: true);
			}
			return oVRAnchor.GetComponent<OVRLocatable>().SetEnabledAsync(enabled: true, timeout);
		}

		public void BindTo(OVRSpatialAnchor spatialAnchor)
		{
			if (!_space.Valid)
			{
				throw new InvalidOperationException("UnboundAnchor does not refer to a valid anchor.");
			}
			if (spatialAnchor == null)
			{
				throw new ArgumentNullException("spatialAnchor");
			}
			if (spatialAnchor.Created)
			{
				throw new ArgumentException(string.Format("Cannot bind {0} to {1} because {2} is already bound to {3}.", Uuid, "spatialAnchor", "spatialAnchor", spatialAnchor.Uuid), "spatialAnchor");
			}
			if (spatialAnchor.PendingCreation)
			{
				throw new ArgumentException(string.Format("Cannot bind {0} to {1} because {2} is being used to create a new spatial anchor.", Uuid, "spatialAnchor", "spatialAnchor"), "spatialAnchor");
			}
			ThrowIfBound(Uuid);
			spatialAnchor.InitializeUnchecked(_space, Uuid);
		}

		internal UnboundAnchor(OVRSpace space, Guid uuid)
		{
			_space = space;
			Uuid = uuid;
		}

		[Obsolete("Use LocalizeAsync instead.")]
		public void Localize(Action<UnboundAnchor, bool> onComplete = null, double timeout = 0.0)
		{
			OVRTask<bool> task = LocalizeAsync(timeout);
			if (onComplete != null)
			{
				InvertedCapture<bool, UnboundAnchor>.ContinueTaskWith(task, onComplete, this);
			}
		}
	}

	private enum MultiAnchorActionType
	{
		Save,
		Share
	}

	private static class Development
	{
		[Conditional("DEVELOPMENT_BUILD")]
		[Conditional("UNITY_EDITOR")]
		public static void Log(string message)
		{
			UnityEngine.Debug.Log("[OVRSpatialAnchor] " + message);
		}

		[Conditional("DEVELOPMENT_BUILD")]
		[Conditional("UNITY_EDITOR")]
		public static void LogWarning(string message)
		{
			UnityEngine.Debug.LogWarning("[OVRSpatialAnchor] " + message);
		}

		[Conditional("DEVELOPMENT_BUILD")]
		[Conditional("UNITY_EDITOR")]
		public static void LogError(string message)
		{
			UnityEngine.Debug.LogError("[OVRSpatialAnchor] " + message);
		}

		[Conditional("DEVELOPMENT_BUILD")]
		[Conditional("UNITY_EDITOR")]
		public static void LogRequestOrError(ulong requestId, OVRPlugin.Result result, string successMessage, string failureMessage)
		{
			result.IsSuccess();
		}

		[Conditional("DEVELOPMENT_BUILD")]
		public static void LogRequest(ulong requestId, string message)
		{
		}

		[Conditional("DEVELOPMENT_BUILD")]
		public static void LogRequestResult(ulong requestId, bool result, string successMessage, string failureMessage)
		{
		}
	}

	[OVRResultStatus]
	public enum OperationResult
	{
		Success = 0,
		Failure = -1000,
		Failure_DataIsInvalid = -1008,
		Failure_InvalidParameter = -1001,
		Failure_SpaceCloudStorageDisabled = -2000,
		Failure_SpaceMappingInsufficient = -2001,
		Failure_SpaceLocalizationFailed = -2002,
		Failure_SpaceNetworkTimeout = -2003,
		Failure_SpaceNetworkRequestFailed = -2004,
		Failure_GroupNotFound = -2009
	}

	private readonly struct InvertedCapture<TResult, TCapture>
	{
		private static readonly Action<TResult, InvertedCapture<TResult, TCapture>> s_delegate = Invoke;

		private readonly TCapture _capture;

		private readonly Action<TCapture, TResult> _callback;

		private InvertedCapture(Action<TCapture, TResult> callback, TCapture capture)
		{
			_callback = callback;
			_capture = capture;
		}

		private static void Invoke(TResult result, InvertedCapture<TResult, TCapture> invertedCapture)
		{
			invertedCapture._callback?.Invoke(invertedCapture._capture, result);
		}

		public static void ContinueTaskWith(OVRTask<TResult> task, Action<TCapture, TResult> onCompleted, TCapture state)
		{
			task.ContinueWith(s_delegate, new InvertedCapture<TResult, TCapture>(onCompleted, state));
		}
	}

	[Obsolete("Use EraseAnchorAsync instead, which does not require you to provide EraseOptions.")]
	public struct EraseOptions
	{
		public OVRSpace.StorageLocation Storage;
	}

	[Obsolete("Use SaveAnchorAsync instead, which does not require you to provide SaveOptions.")]
	public struct SaveOptions
	{
		public OVRSpace.StorageLocation Storage;
	}

	[Obsolete("Only for use with the obsolete version of LoadUnboundAnchorsAsync. Use the overload of LoadUnboundAnchorsAsync that accepts a collection of Guids")]
	public struct LoadOptions
	{
		public const int MaxSupported = 1024;

		private IReadOnlyList<Guid> _uuids;

		public OVRSpace.StorageLocation StorageLocation { get; set; }

		[Obsolete("This property is no longer required. MaxAnchorCount will be automatically set to the number of uuids to load.")]
		public int MaxAnchorCount { get; set; }

		public double Timeout { get; set; }

		public IReadOnlyList<Guid> Uuids
		{
			get
			{
				return _uuids;
			}
			set
			{
				if (value != null && value.Count > 1024)
				{
					throw new ArgumentException($"There must not be more than {1024} UUIDs (new value contains {value.Count} UUIDs).", "value");
				}
				_uuids = value;
			}
		}

		internal OVRSpaceQuery.Options ToQueryOptions()
		{
			OVRSpaceQuery.Options result = default(OVRSpaceQuery.Options);
			result.Location = StorageLocation;
			result.MaxResults = 1024;
			result.Timeout = Timeout;
			result.UuidFilter = Uuids;
			result.QueryType = OVRPlugin.SpaceQueryType.Action;
			result.ActionType = OVRPlugin.SpaceQueryActionType.Load;
			return result;
		}
	}

	private bool _startCalled;

	private ulong _requestId;

	private bool _creationFailed;

	internal static readonly Dictionary<Guid, OVRSpatialAnchor> SpatialAnchors;

	private static readonly Dictionary<ulong, OVRSpatialAnchor> CreationRequests;

	private static readonly Dictionary<OVRSpatialAnchor, Guid> AsyncRequestTaskIds;

	private static readonly List<(List<OVRSpaceUser>, List<OVRSpatialAnchor>)> ShareRequests;

	private static readonly Dictionary<ulong, MultiAnchorDelegatePair> MultiAnchorCompletionDelegates;

	[Obsolete("See SaveAnchorAsync overload without SaveOptions")]
	private readonly SaveOptions _defaultSaveOptions = new SaveOptions
	{
		Storage = OVRSpace.StorageLocation.Local
	};

	[Obsolete("See EraseAnchorAsync overload without EraseOptions")]
	private readonly EraseOptions _defaultEraseOptions = new EraseOptions
	{
		Storage = OVRSpace.StorageLocation.Local
	};

	[Obsolete]
	private static readonly Dictionary<OVRSpace.StorageLocation, List<OVRSpatialAnchor>> SaveRequests;

	internal OVRAnchor _anchor { get; private set; }

	public Guid Uuid => _anchor.Uuid;

	public bool Created
	{
		get
		{
			if ((bool)this)
			{
				return _anchor != OVRAnchor.Null;
			}
			return false;
		}
	}

	public bool PendingCreation
	{
		get
		{
			if ((bool)this)
			{
				return _requestId != 0;
			}
			return false;
		}
	}

	public bool Localized
	{
		get
		{
			bool flag = default(bool);
			bool changePending;
			return Created && OVRPlugin.GetSpaceComponentStatus(_anchor.Handle, OVRPlugin.SpaceComponentType.Locatable, out flag, out changePending) && flag;
		}
	}

	[Obsolete("This property exposes an internal handle that should no longer be necessary. You can Save, Erase, and Share anchors using the methods in this class.")]
	public OVRSpace Space => _anchor.Handle;

	private event Action<OperationResult> _onLocalize;

	public event Action<OperationResult> OnLocalize
	{
		add
		{
			if (Created && OVRPlugin.GetSpaceComponentStatus(_anchor.Handle, OVRPlugin.SpaceComponentType.Locatable, out var flag, out var changePending) && !changePending)
			{
				value((!flag) ? OperationResult.Failure : OperationResult.Success);
			}
			else
			{
				_onLocalize += value;
			}
		}
		remove
		{
			_onLocalize -= value;
		}
	}

	public async OVRTask<bool> WhenCreatedAsync()
	{
		while ((bool)this && !Created && !_creationFailed)
		{
			await Task.Yield();
		}
		return (bool)this && Created;
	}

	public async OVRTask<bool> WhenLocalizedAsync()
	{
		if (!(await WhenCreatedAsync()))
		{
			return false;
		}
		bool result;
		bool changePending;
		while (OVRPlugin.GetSpaceComponentStatus(_anchor.Handle, OVRPlugin.SpaceComponentType.Locatable, out result, out changePending))
		{
			if (!changePending)
			{
				return result;
			}
			await Task.Yield();
		}
		return false;
	}

	public OVRTask<OperationResult> ShareAsync(OVRSpaceUser user)
	{
		List<OVRSpaceUser> list = OVRObjectPool.List<OVRSpaceUser>();
		list.Add(user);
		return ShareAsyncInternal(list);
	}

	public OVRTask<OperationResult> ShareAsync(OVRSpaceUser user1, OVRSpaceUser user2)
	{
		List<OVRSpaceUser> list = OVRObjectPool.List<OVRSpaceUser>();
		list.Add(user1);
		list.Add(user2);
		return ShareAsyncInternal(list);
	}

	public OVRTask<OperationResult> ShareAsync(OVRSpaceUser user1, OVRSpaceUser user2, OVRSpaceUser user3)
	{
		List<OVRSpaceUser> list = OVRObjectPool.List<OVRSpaceUser>();
		list.Add(user1);
		list.Add(user2);
		list.Add(user3);
		return ShareAsyncInternal(list);
	}

	public OVRTask<OperationResult> ShareAsync(OVRSpaceUser user1, OVRSpaceUser user2, OVRSpaceUser user3, OVRSpaceUser user4)
	{
		List<OVRSpaceUser> list = OVRObjectPool.List<OVRSpaceUser>();
		list.Add(user1);
		list.Add(user2);
		list.Add(user3);
		list.Add(user4);
		return ShareAsyncInternal(list);
	}

	public OVRTask<OperationResult> ShareAsync(IEnumerable<OVRSpaceUser> users)
	{
		List<OVRSpaceUser> list = OVRObjectPool.List<OVRSpaceUser>();
		list.AddRange(users);
		return ShareAsyncInternal(list);
	}

	public unsafe OVRTask<OVRResult<OVRAnchor.ShareResult>> ShareAsync(Guid groupUuid)
	{
		if (groupUuid == Guid.Empty)
		{
			throw new ArgumentException("groupUuid must not be a 0 uuid", "groupUuid");
		}
		ulong handle = _anchor.Handle;
		ReadOnlySpan<ulong> anchors = new ReadOnlySpan<ulong>(&handle, 1);
		ReadOnlySpan<Guid> groupUuids = new ReadOnlySpan<Guid>(&groupUuid, 1);
		return OVRAnchor.ShareAsyncInternal(anchors, groupUuids);
	}

	public unsafe static OVRTask<OperationResult> ShareAsync(IEnumerable<OVRSpatialAnchor> anchors, IEnumerable<OVRSpaceUser> users)
	{
		if (anchors == null)
		{
			throw new ArgumentNullException("anchors");
		}
		if (users == null)
		{
			throw new ArgumentNullException("users");
		}
		using OVRNativeList<ulong> oVRNativeList = new OVRNativeList<ulong>(anchors.ToNonAlloc().Count, Allocator.Temp);
		foreach (OVRSpatialAnchor item in anchors.ToNonAlloc())
		{
			oVRNativeList.Add(item._anchor.Handle);
		}
		using OVRNativeList<ulong> oVRNativeList2 = new OVRNativeList<ulong>(users.ToNonAlloc().Count, Allocator.Temp);
		foreach (OVRSpaceUser item2 in users.ToNonAlloc())
		{
			oVRNativeList2.Add(item2._handle);
		}
		ulong requestId;
		return OVRTask.Build(OVRPlugin.ShareSpaces(oVRNativeList, (uint)oVRNativeList.Count, oVRNativeList2, (uint)oVRNativeList2.Count, out requestId), requestId).ToTask<OperationResult>();
	}

	public unsafe static OVRTask<OVRResult<OVRAnchor.ShareResult>> ShareAsync(IEnumerable<OVRSpatialAnchor> anchors, Guid groupUuid)
	{
		if (anchors == null)
		{
			throw new ArgumentNullException("anchors");
		}
		if (groupUuid == Guid.Empty)
		{
			throw new ArgumentException("groupUuid must not be a 0 uuid", "groupUuid");
		}
		OVREnumerable<OVRSpatialAnchor> oVREnumerable = anchors.ToNonAlloc();
		using OVRNativeList<ulong> oVRNativeList = new OVRNativeList<ulong>(oVREnumerable.Count, Allocator.Temp);
		foreach (OVRSpatialAnchor item in oVREnumerable)
		{
			oVRNativeList.Add(item._anchor.Handle);
		}
		return OVRAnchor.ShareAsyncInternal(groupUuids: new ReadOnlySpan<Guid>(&groupUuid, 1), anchors: oVRNativeList);
	}

	public static OVRTask<OVRResult<OVRAnchor.ShareResult>> ShareAsync(IEnumerable<OVRSpatialAnchor> anchors, IEnumerable<Guid> groupUuids)
	{
		if (anchors == null)
		{
			throw new ArgumentNullException("anchors");
		}
		if (groupUuids == null)
		{
			throw new ArgumentNullException("groupUuids");
		}
		OVREnumerable<OVRSpatialAnchor> oVREnumerable = anchors.ToNonAlloc();
		using OVRNativeList<ulong> oVRNativeList = new OVRNativeList<ulong>(oVREnumerable.Count, Allocator.Temp);
		foreach (OVRSpatialAnchor item in oVREnumerable)
		{
			oVRNativeList.Add(item._anchor.Handle);
		}
		using OVRNativeList<Guid> oVRNativeList2 = groupUuids.ToNativeList(Allocator.Temp);
		foreach (Guid item2 in oVRNativeList2)
		{
			if (item2 == Guid.Empty)
			{
				throw new ArgumentException("groupUuids must not contain a 0 uuid", "groupUuids");
			}
		}
		return OVRAnchor.ShareAsyncInternal(oVRNativeList, oVRNativeList2);
	}

	private OVRTask<OperationResult> ShareAsyncInternal(List<OVRSpaceUser> users)
	{
		GetListToStoreTheShareRequest(users).Add(this);
		Guid guid = Guid.NewGuid();
		AsyncRequestTaskIds[this] = guid;
		return OVRTask.FromGuid<OperationResult>(guid);
	}

	private List<OVRSpatialAnchor> GetListToStoreTheShareRequest(List<OVRSpaceUser> users)
	{
		users.Sort((OVRSpaceUser x, OVRSpaceUser y) => x.Id.CompareTo(y.Id));
		foreach (var (sortedList, result) in ShareRequests)
		{
			if (AreSortedUserListsEqual(users, sortedList))
			{
				return result;
			}
		}
		List<OVRSpatialAnchor> list = OVRObjectPool.List<OVRSpatialAnchor>();
		ShareRequests.Add((users, list));
		return list;
	}

	private static bool AreSortedUserListsEqual(IReadOnlyList<OVRSpaceUser> sortedList1, IReadOnlyList<OVRSpaceUser> sortedList2)
	{
		if (sortedList1.Count != sortedList2.Count)
		{
			return false;
		}
		for (int i = 0; i < sortedList1.Count; i++)
		{
			if (sortedList1[i].Id != sortedList2[i].Id)
			{
				return false;
			}
		}
		return true;
	}

	public static OVRTask<OVRResult<OVRAnchor.SaveResult>> SaveAnchorsAsync(IEnumerable<OVRSpatialAnchor> anchors)
	{
		if (anchors == null)
		{
			throw new ArgumentNullException("anchors");
		}
		using OVRNativeList<ulong> oVRNativeList = new OVRNativeList<ulong>(Allocator.Temp);
		foreach (OVRSpatialAnchor item in anchors.ToNonAlloc())
		{
			oVRNativeList.Add(item._anchor.Handle);
		}
		return OVRAnchor.SaveSpacesAsync(oVRNativeList);
	}

	public OVRTask<OVRResult<OVRAnchor.SaveResult>> SaveAnchorAsync()
	{
		return _anchor.SaveAsync();
	}

	public OVRTask<OVRResult<OVRAnchor.EraseResult>> EraseAnchorAsync()
	{
		return _anchor.EraseAsync();
	}

	public static OVRTask<OVRResult<OVRAnchor.EraseResult>> EraseAnchorsAsync(IEnumerable<OVRSpatialAnchor> anchors, IEnumerable<Guid> uuids)
	{
		if (anchors == null && uuids == null)
		{
			throw new ArgumentException("One of anchors or uuids must not be null.");
		}
		List<OVRAnchor> list;
		using (new OVRObjectPool.ListScope<OVRAnchor>(out list))
		{
			foreach (OVRSpatialAnchor item in anchors.ToNonAlloc())
			{
				list.Add(item._anchor);
			}
			return OVRAnchor.EraseAsync(list, uuids);
		}
	}

	private static void ThrowIfBound(Guid uuid)
	{
		if (SpatialAnchors.ContainsKey(uuid))
		{
			throw new InvalidOperationException(string.Format("Spatial anchor with uuid {0} is already bound to an {1}.", uuid, "OVRSpatialAnchor"));
		}
	}

	private void InitializeUnchecked(OVRSpace space, Guid uuid)
	{
		SpatialAnchors.Add(uuid, this);
		_requestId = 0uL;
		_anchor = new OVRAnchor(space, uuid);
		if (_anchor.TryGetComponent<OVRLocatable>(out var component))
		{
			component.SetEnabledAsync(enabled: true);
		}
		if (_anchor.TryGetComponent<OVRStorable>(out var component2))
		{
			component2.SetEnabledAsync(enabled: true);
		}
		if (_anchor.TryGetComponent<OVRSharable>(out var component3))
		{
			component3.SetEnabledAsync(enabled: true);
		}
		UpdateTransform();
	}

	private void Start()
	{
		_startCalled = true;
		if (!Created)
		{
			CreateSpatialAnchor();
		}
	}

	private void Update()
	{
		if (Created)
		{
			UpdateTransform();
		}
	}

	private void LateUpdate()
	{
		SaveBatchAnchors();
		ShareBatchAnchors();
	}

	private static void ShareBatchAnchors()
	{
		foreach (var (list, list2) in ShareRequests)
		{
			if (list.Count > 0 && list2.Count > 0)
			{
				Share(list2, list);
			}
			OVRObjectPool.Return(list);
			OVRObjectPool.Return(list2);
		}
		ShareRequests.Clear();
	}

	private void OnDestroy()
	{
		if (_anchor != OVRAnchor.Null)
		{
			_anchor.Dispose();
		}
		SpatialAnchors.Remove(Uuid);
	}

	private OVRPose GetTrackingSpacePose()
	{
		Camera main = Camera.main;
		if ((bool)main)
		{
			return base.transform.ToTrackingSpacePose(main);
		}
		return base.transform.ToOVRPose();
	}

	private void CreateSpatialAnchor()
	{
		OVRPlugin.SpatialAnchorCreateInfo createInfo = default(OVRPlugin.SpatialAnchorCreateInfo);
		createInfo.BaseTracking = OVRPlugin.GetTrackingOriginType();
		createInfo.PoseInSpace = GetTrackingSpacePose().ToPosef();
		createInfo.Time = OVRPlugin.GetTimeInSeconds();
		if (OVRPlugin.CreateSpatialAnchor(createInfo, out _requestId))
		{
			CreationRequests[_requestId] = this;
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	internal static bool TryGetPose(OVRSpace space, out OVRPose pose)
	{
		if (!OVRPlugin.TryLocateSpace(space, OVRPlugin.GetTrackingOriginType(), out var pose2, out var locationFlags) || !locationFlags.IsOrientationValid() || !locationFlags.IsPositionValid())
		{
			pose = OVRPose.identity;
			return false;
		}
		pose = pose2.ToOVRPose();
		Camera main = Camera.main;
		if ((bool)main)
		{
			pose = pose.ToWorldSpacePose(main);
		}
		return true;
	}

	private void UpdateTransform()
	{
		if (TryGetPose(_anchor.Handle, out var pose))
		{
			base.transform.SetPositionAndRotation(pose.position, pose.orientation);
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void InitializeOnLoad()
	{
		CreationRequests.Clear();
		MultiAnchorCompletionDelegates.Clear();
		SpatialAnchors.Clear();
	}

	static OVRSpatialAnchor()
	{
		SpatialAnchors = new Dictionary<Guid, OVRSpatialAnchor>();
		CreationRequests = new Dictionary<ulong, OVRSpatialAnchor>();
		AsyncRequestTaskIds = new Dictionary<OVRSpatialAnchor, Guid>();
		ShareRequests = new List<(List<OVRSpaceUser>, List<OVRSpatialAnchor>)>();
		MultiAnchorCompletionDelegates = new Dictionary<ulong, MultiAnchorDelegatePair>();
		SaveRequests = new Dictionary<OVRSpace.StorageLocation, List<OVRSpatialAnchor>>
		{
			{
				OVRSpace.StorageLocation.Cloud,
				new List<OVRSpatialAnchor>()
			},
			{
				OVRSpace.StorageLocation.Local,
				new List<OVRSpatialAnchor>()
			}
		};
		OVRManager.SpatialAnchorCreateComplete += OnSpatialAnchorCreateComplete;
		OVRManager.SpaceSaveComplete += OnSpaceSaveComplete;
		OVRManager.SpaceListSaveComplete += OnSpaceListSaveComplete;
		OVRManager.ShareSpacesComplete += OnShareSpacesComplete;
		OVRManager.SpaceEraseComplete += OnSpaceEraseComplete;
		OVRManager.SpaceQueryComplete += OnSpaceQueryComplete;
		OVRManager.SpaceSetComponentStatusComplete += OnSpaceSetComponentStatusComplete;
	}

	private static void InvokeMultiAnchorDelegate(ulong requestId, OperationResult result, MultiAnchorActionType actionType)
	{
		if (!MultiAnchorCompletionDelegates.Remove(requestId, out var value))
		{
			return;
		}
		value.Delegate?.Invoke(value.Anchors, result);
		try
		{
			foreach (OVRSpatialAnchor anchor in value.Anchors)
			{
				switch (actionType)
				{
				case MultiAnchorActionType.Save:
				{
					if (AsyncRequestTaskIds.Remove(anchor, out var value3))
					{
						OVRTask.SetResult(value3, result == OperationResult.Success);
					}
					break;
				}
				case MultiAnchorActionType.Share:
				{
					if (AsyncRequestTaskIds.Remove(anchor, out var value2))
					{
						OVRTask.SetResult(value2, result);
					}
					break;
				}
				default:
					throw new ArgumentOutOfRangeException("actionType", actionType, null);
				}
			}
		}
		finally
		{
			OVRObjectPool.Return(value.Anchors);
		}
	}

	private static void OnSpatialAnchorCreateComplete(ulong requestId, bool success, OVRSpace space, Guid uuid)
	{
		if (CreationRequests.Remove(requestId, out var value))
		{
			if ((bool)value)
			{
				value._creationFailed = !success;
			}
			if (success && (bool)value)
			{
				value.InitializeUnchecked(space, uuid);
			}
			else if (success && !value)
			{
				OVRPlugin.DestroySpace(space);
			}
			else if (!success && (bool)value)
			{
				UnityEngine.Object.Destroy(value);
			}
		}
	}

	public static OVRTask<OVRResult<List<UnboundAnchor>, OVRAnchor.FetchResult>> LoadUnboundAnchorsAsync(IEnumerable<Guid> uuids, List<UnboundAnchor> unboundAnchors, Action<List<UnboundAnchor>, int> onIncrementalResultsAvailable = null)
	{
		if (uuids == null)
		{
			throw new ArgumentNullException("uuids");
		}
		if (unboundAnchors == null)
		{
			throw new ArgumentNullException("unboundAnchors");
		}
		OVRAnchor.FetchOptions fetchOptions = default(OVRAnchor.FetchOptions);
		fetchOptions.Uuids = uuids;
		return LoadUnboundAnchorsAsync(fetchOptions, unboundAnchors, onIncrementalResultsAvailable);
	}

	public static async OVRTask<OVRResult<List<UnboundAnchor>, OperationResult>> LoadUnboundSharedAnchorsAsync(IEnumerable<Guid> uuids, List<UnboundAnchor> unboundAnchors)
	{
		if (uuids == null)
		{
			throw new ArgumentNullException("uuids");
		}
		if (unboundAnchors == null)
		{
			throw new ArgumentNullException("unboundAnchors");
		}
		OVRPlugin.SpaceQueryInfo2 queryInfo = OVRSpaceQuery.ForAnchorsThrow(uuids, "uuids");
		unboundAnchors.Clear();
		List<OVRAnchor> anchorBuff;
		using (new OVRObjectPool.ListScope<OVRAnchor>(out anchorBuff))
		{
			OVRPlugin.Result result = await OVRAnchor.FetchAnchors(anchorBuff, queryInfo);
			if (!result.IsSuccess())
			{
				return OVRResult.From(unboundAnchors, (OperationResult)result);
			}
			foreach (OVRAnchor item in anchorBuff)
			{
				if (TryGetUnbound(item, out var unboundAnchor))
				{
					unboundAnchors.Add(unboundAnchor);
				}
			}
			return OVRResult.From(unboundAnchors, (OperationResult)result);
		}
	}

	public static async OVRTask<OVRResult<List<UnboundAnchor>, OperationResult>> LoadUnboundSharedAnchorsAsync(Guid groupUuid, List<UnboundAnchor> unboundAnchors)
	{
		if (groupUuid == Guid.Empty)
		{
			throw new ArgumentException("groupUuid must not be a 0 uuid", "groupUuid");
		}
		if (unboundAnchors == null)
		{
			throw new ArgumentNullException("unboundAnchors");
		}
		OVRPlugin.SpaceQueryInfo2 queryInfo = OVRSpaceQuery.ForGroupThrow(groupUuid, "groupUuid");
		unboundAnchors.Clear();
		List<OVRAnchor> anchorBuff;
		using (new OVRObjectPool.ListScope<OVRAnchor>(out anchorBuff))
		{
			OVRPlugin.Result result = await OVRAnchor.FetchAnchors(anchorBuff, queryInfo);
			if (!result.IsSuccess())
			{
				return OVRResult.From(unboundAnchors, (OperationResult)result);
			}
			foreach (OVRAnchor item in anchorBuff)
			{
				if (TryGetUnbound(item, out var unboundAnchor))
				{
					unboundAnchors.Add(unboundAnchor);
				}
			}
			return OVRResult.From(unboundAnchors, (OperationResult)result);
		}
	}

	public static async OVRTask<OVRResult<List<UnboundAnchor>, OperationResult>> LoadUnboundSharedAnchorsAsync(Guid groupUuid, IEnumerable<Guid> allowedAnchorUuids, List<UnboundAnchor> unboundAnchors)
	{
		if (groupUuid == Guid.Empty)
		{
			throw new ArgumentException("groupUuid must not be a 0 uuid", "groupUuid");
		}
		if (allowedAnchorUuids == null)
		{
			throw new ArgumentNullException("allowedAnchorUuids");
		}
		if (unboundAnchors == null)
		{
			throw new ArgumentNullException("unboundAnchors");
		}
		unboundAnchors.Clear();
		OVRPlugin.SpaceQueryInfo2 queryInfo = OVRSpaceQuery.ForGroupThrow(groupUuid, "groupUuid", allowedAnchorUuids);
		List<OVRAnchor> anchorBuff;
		using (new OVRObjectPool.ListScope<OVRAnchor>(out anchorBuff))
		{
			OVRPlugin.Result result = await OVRAnchor.FetchAnchors(anchorBuff, queryInfo);
			if (!result.IsSuccess())
			{
				return OVRResult.From(unboundAnchors, (OperationResult)result);
			}
			foreach (OVRAnchor item in anchorBuff)
			{
				if (TryGetUnbound(item, out var unboundAnchor))
				{
					unboundAnchors.Add(unboundAnchor);
				}
			}
			return OVRResult.From(unboundAnchors, (OperationResult)result);
		}
	}

	private static async OVRTask<OVRResult<List<UnboundAnchor>, OVRAnchor.FetchResult>> LoadUnboundAnchorsAsync(OVRAnchor.FetchOptions fetchOptions, List<UnboundAnchor> unboundAnchors, Action<List<UnboundAnchor>, int> resultsHandler)
	{
		unboundAnchors.Clear();
		List<OVRAnchor> list;
		OVRAnchor.FetchResult status;
		using (new OVRObjectPool.ListScope<OVRAnchor>(out list))
		{
			OVRResult<List<OVRAnchor>, OVRAnchor.FetchResult> oVRResult = await OVRAnchor.FetchAnchorsAsync(list, fetchOptions, (resultsHandler == null) ? null : ((Action<List<OVRAnchor>, int>)delegate(List<OVRAnchor> incrementalResults, int staringIndex)
			{
				int? num = null;
				unboundAnchors.Clear();
				for (int i = 0; i < incrementalResults.Count; i++)
				{
					if (TryGetUnbound(incrementalResults[i], out var unboundAnchor2))
					{
						if (i >= staringIndex && !num.HasValue)
						{
							num = unboundAnchors.Count;
						}
						unboundAnchors.Add(unboundAnchor2);
					}
				}
				if (num.HasValue)
				{
					resultsHandler(unboundAnchors, num.Value);
				}
			}));
			status = oVRResult.Status;
			unboundAnchors.Clear();
			if (oVRResult.Success)
			{
				foreach (OVRAnchor item in oVRResult.Value)
				{
					if (TryGetUnbound(item, out var unboundAnchor))
					{
						unboundAnchors.Add(unboundAnchor);
					}
				}
			}
		}
		return OVRResult.From(unboundAnchors, status);
	}

	public static bool FromOVRAnchor(OVRAnchor anchor, out UnboundAnchor unboundAnchor)
	{
		if (anchor == OVRAnchor.Null)
		{
			throw new ArgumentNullException("anchor");
		}
		return TryGetUnbound(anchor, out unboundAnchor);
	}

	private static bool TryGetUnbound(OVRAnchor anchor, out UnboundAnchor unboundAnchor)
	{
		unboundAnchor = new UnboundAnchor(anchor.Handle, anchor.Uuid);
		if (SpatialAnchors.TryGetValue(unboundAnchor.Uuid, out var _))
		{
			return false;
		}
		OVRLocatable component;
		bool num = anchor.TryGetComponent<OVRLocatable>(out component);
		_ = string.Empty;
		if (!num)
		{
			return false;
		}
		return true;
	}

	private static void OnSpaceSetComponentStatusComplete(ulong requestId, bool result, OVRSpace space, Guid uuid, OVRPlugin.SpaceComponentType componentType, bool enabled)
	{
		if (componentType == OVRPlugin.SpaceComponentType.Locatable && SpatialAnchors.TryGetValue(uuid, out var value))
		{
			value._onLocalize?.Invoke((!enabled) ? OperationResult.Failure : OperationResult.Success);
		}
	}

	private static void OnShareSpacesComplete(ulong requestId, OperationResult result)
	{
		OVRTask.SetResult(requestId, result);
		InvokeMultiAnchorDelegate(requestId, result, MultiAnchorActionType.Share);
	}

	[Obsolete("You should use LoadUnboundAnchorsAsync to load previously saved anchors and AddComponent<OVRSpatialAnchor>() to create a new anchor. You should no longer need to use an OVRSpace handle directly.")]
	public void InitializeFromExisting(OVRSpace space, Guid uuid)
	{
		if (_startCalled)
		{
			throw new InvalidOperationException("Cannot call InitializeFromExisting after Start. This must be set once upon creation.");
		}
		try
		{
			if (!space.Valid)
			{
				throw new ArgumentException($"Invalid space {space}.", "space");
			}
			ThrowIfBound(uuid);
		}
		catch
		{
			UnityEngine.Object.Destroy(this);
			throw;
		}
		InitializeUnchecked(space, uuid);
	}

	[Obsolete("Use SaveAsync instead.")]
	public void Save(Action<OVRSpatialAnchor, bool> onComplete = null)
	{
		Save(_defaultSaveOptions, onComplete);
	}

	[Obsolete("Use SaveAsync instead.")]
	public void Save(SaveOptions saveOptions, Action<OVRSpatialAnchor, bool> onComplete = null)
	{
		OVRTask<bool> task = SaveAsync(saveOptions);
		if (onComplete != null)
		{
			InvertedCapture<bool, OVRSpatialAnchor>.ContinueTaskWith(task, onComplete, this);
		}
	}

	[Obsolete("Use ShareAsync instead.")]
	public void Share(OVRSpaceUser user, Action<OperationResult> onComplete = null)
	{
		OVRTask<OperationResult> oVRTask = ShareAsync(user);
		if (onComplete != null)
		{
			oVRTask.ContinueWith(onComplete);
		}
	}

	[Obsolete("Use ShareAsync instead.")]
	public void Share(OVRSpaceUser user1, OVRSpaceUser user2, Action<OperationResult> onComplete = null)
	{
		OVRTask<OperationResult> oVRTask = ShareAsync(user1, user2);
		if (onComplete != null)
		{
			oVRTask.ContinueWith(onComplete);
		}
	}

	[Obsolete("Use ShareAsync instead.")]
	public void Share(OVRSpaceUser user1, OVRSpaceUser user2, OVRSpaceUser user3, Action<OperationResult> onComplete = null)
	{
		OVRTask<OperationResult> oVRTask = ShareAsync(user1, user2, user3);
		if (onComplete != null)
		{
			oVRTask.ContinueWith(onComplete);
		}
	}

	[Obsolete("Use ShareAsync instead.")]
	public void Share(OVRSpaceUser user1, OVRSpaceUser user2, OVRSpaceUser user3, OVRSpaceUser user4, Action<OperationResult> onComplete = null)
	{
		OVRTask<OperationResult> oVRTask = ShareAsync(user1, user2, user3, user4);
		if (onComplete != null)
		{
			oVRTask.ContinueWith(onComplete);
		}
	}

	[Obsolete("Use ShareAsync instead.")]
	public void Share(IEnumerable<OVRSpaceUser> users, Action<OperationResult> onComplete = null)
	{
		OVRTask<OperationResult> oVRTask = ShareAsync(users);
		if (onComplete != null)
		{
			oVRTask.ContinueWith(onComplete);
		}
	}

	[Obsolete("Use EraseAsync instead.")]
	public void Erase(Action<OVRSpatialAnchor, bool> onComplete = null)
	{
		Erase(_defaultEraseOptions, onComplete);
	}

	[Obsolete("Use EraseAsync instead.")]
	public void Erase(EraseOptions eraseOptions, Action<OVRSpatialAnchor, bool> onComplete = null)
	{
		OVRTask<bool> task = EraseAsync(eraseOptions);
		if (onComplete != null)
		{
			InvertedCapture<bool, OVRSpatialAnchor>.ContinueTaskWith(task, onComplete, this);
		}
	}

	[Obsolete("Use LoadUnboundAnchorsAsync instead.")]
	public static bool LoadUnboundAnchors(LoadOptions options, Action<UnboundAnchor[]> onComplete)
	{
		OVRTask<UnboundAnchor[]> oVRTask = LoadUnboundAnchorsAsync(options);
		oVRTask.ContinueWith(onComplete);
		return oVRTask.IsPending;
	}

	[Obsolete("Use ShareAsync instead.")]
	public static void Share(ICollection<OVRSpatialAnchor> anchors, ICollection<OVRSpaceUser> users, Action<ICollection<OVRSpatialAnchor>, OperationResult> onComplete = null)
	{
		if (anchors == null)
		{
			throw new ArgumentNullException("anchors");
		}
		if (users == null)
		{
			throw new ArgumentNullException("users");
		}
		using NativeArray<ulong> spaces = ToNativeArray(anchors);
		NativeArray<ulong> nativeArray = new NativeArray<ulong>(users.Count, Allocator.Temp);
		using (nativeArray)
		{
			int num = 0;
			foreach (OVRSpaceUser user in users)
			{
				nativeArray[num++] = user._handle;
			}
			ulong requestId;
			OVRPlugin.Result result = OVRPlugin.ShareSpaces(spaces, nativeArray, out requestId);
			if (result.IsSuccess())
			{
				MultiAnchorCompletionDelegates[requestId] = new MultiAnchorDelegatePair
				{
					Anchors = CopyAnchorListIntoListFromPool(anchors),
					Delegate = onComplete
				};
			}
			else
			{
				onComplete?.Invoke(anchors, (OperationResult)result);
			}
		}
	}

	[Obsolete("Use SaveAsync instead.")]
	public unsafe static void Save(ICollection<OVRSpatialAnchor> anchors, SaveOptions saveOptions, Action<ICollection<OVRSpatialAnchor>, OperationResult> onComplete = null)
	{
		if (anchors == null)
		{
			throw new ArgumentNullException("anchors");
		}
		using NativeArray<ulong> nativeArray = ToNativeArray(anchors);
		ulong requestId;
		OVRPlugin.Result result = OVRAnchor.SaveSpaceList((ulong*)nativeArray.GetUnsafeReadOnlyPtr(), (uint)nativeArray.Length, saveOptions.Storage.ToSpaceStorageLocation(), out requestId);
		if (result.IsSuccess())
		{
			MultiAnchorCompletionDelegates[requestId] = new MultiAnchorDelegatePair
			{
				Anchors = CopyAnchorListIntoListFromPool(anchors),
				Delegate = onComplete
			};
		}
		else
		{
			onComplete?.Invoke(anchors, (OperationResult)result);
		}
	}

	[Obsolete("Use EraseAnchorAsync instead.")]
	public OVRTask<bool> EraseAsync()
	{
		return EraseAsync(_defaultEraseOptions);
	}

	[Obsolete("Use EraseAnchorAsync instead.")]
	public OVRTask<bool> EraseAsync(EraseOptions eraseOptions)
	{
		ulong requestId;
		return OVRTask.Build(OVRAnchor.EraseSpace(_anchor.Handle, eraseOptions.Storage.ToSpaceStorageLocation(), out requestId), requestId).ToTask(failureValue: false);
	}

	[Obsolete("Use SaveAnchorAsync instead.")]
	public OVRTask<bool> SaveAsync()
	{
		return SaveAsync(_defaultSaveOptions);
	}

	[Obsolete("Use SaveAnchorAsync instead.")]
	public OVRTask<bool> SaveAsync(SaveOptions saveOptions)
	{
		Guid guid = Guid.NewGuid();
		SaveRequests[saveOptions.Storage].Add(this);
		AsyncRequestTaskIds[this] = guid;
		return OVRTask.FromGuid<bool>(guid);
	}

	[Obsolete("Use SaveAnchorsAsync instead.")]
	public unsafe static OVRTask<OperationResult> SaveAsync(IEnumerable<OVRSpatialAnchor> anchors, SaveOptions saveOptions)
	{
		if (anchors == null)
		{
			throw new ArgumentNullException("anchors");
		}
		using OVRNativeList<ulong> oVRNativeList = new OVRNativeList<ulong>(anchors.ToNonAlloc().Count, Allocator.Temp);
		foreach (OVRSpatialAnchor item in anchors.ToNonAlloc())
		{
			oVRNativeList.Add(item._anchor.Handle);
		}
		ulong requestId;
		return OVRTask.Build(OVRAnchor.SaveSpaceList(oVRNativeList, (uint)oVRNativeList.Count, saveOptions.Storage.ToSpaceStorageLocation(), out requestId), requestId).ToTask<OperationResult>();
	}

	[Obsolete("Use the overload of LoadUnboundAnchorsAsync that accepts a collection of Guids instead.")]
	public static OVRTask<UnboundAnchor[]> LoadUnboundAnchorsAsync(LoadOptions options)
	{
		if (options.Uuids == null)
		{
			throw new InvalidOperationException("LoadOptions.Uuids must not be null.");
		}
		ulong requestId;
		return OVRTask.Build(options.ToQueryOptions().TryQuerySpaces(out requestId), requestId).ToTask<UnboundAnchor[]>(null);
	}

	private static NativeArray<ulong> ToNativeArray(ICollection<OVRSpatialAnchor> anchors)
	{
		int count = anchors.Count;
		NativeArray<ulong> result = new NativeArray<ulong>(count, Allocator.Temp);
		int num = 0;
		foreach (OVRSpatialAnchor item in anchors.ToNonAlloc())
		{
			result[num++] = (item ? item._anchor.Handle : 0);
		}
		return result;
	}

	private static List<OVRSpatialAnchor> CopyAnchorListIntoListFromPool(IEnumerable<OVRSpatialAnchor> anchorList)
	{
		List<OVRSpatialAnchor> list = OVRObjectPool.List<OVRSpatialAnchor>();
		list.AddRange(anchorList);
		return list;
	}

	[Obsolete]
	private static void SaveBatchAnchors()
	{
		foreach (KeyValuePair<OVRSpace.StorageLocation, List<OVRSpatialAnchor>> saveRequest in SaveRequests)
		{
			if (saveRequest.Value.Count != 0)
			{
				Save(saveRequest.Value, new SaveOptions
				{
					Storage = saveRequest.Key
				});
				saveRequest.Value.Clear();
			}
		}
	}

	private static void OnSpaceSaveComplete(ulong requestId, OVRSpace space, bool result, Guid uuid)
	{
	}

	private static void OnSpaceEraseComplete(ulong requestId, bool result, Guid uuid, OVRPlugin.SpaceStorageLocation location)
	{
	}

	private static void OnSpaceQueryComplete(ulong requestId, bool queryResult)
	{
		if (!OVRTask.TryGetPendingTask(requestId, out OVRTask<UnboundAnchor[]> task))
		{
			return;
		}
		if (!queryResult)
		{
			task.SetResult(null);
			return;
		}
		if (!OVRPlugin.RetrieveSpaceQueryResults(requestId, out var results, Allocator.Temp))
		{
			task.SetResult(null);
			return;
		}
		using (results)
		{
			List<UnboundAnchor> list;
			using (new OVRObjectPool.ListScope<UnboundAnchor>(out list))
			{
				foreach (OVRPlugin.SpaceQueryResult item in results)
				{
					if (TryGetUnbound(new OVRAnchor(item.space, item.uuid), out var unboundAnchor))
					{
						list.Add(unboundAnchor);
					}
				}
				UnboundAnchor[] result = ((list.Count == 0) ? Array.Empty<UnboundAnchor>() : list.ToArray());
				task.SetResult(result);
			}
		}
	}

	private static void OnSpaceListSaveComplete(ulong requestId, OperationResult result)
	{
		OVRTask.SetResult(requestId, result);
		InvokeMultiAnchorDelegate(requestId, result, MultiAnchorActionType.Save);
	}
}
