using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.XR;

public readonly struct OVRLocatable : IOVRAnchorComponent<OVRLocatable>, IEquatable<OVRLocatable>
{
	public readonly struct TrackingSpacePose
	{
		internal readonly OVRPlugin.SpaceLocationFlags Flags;

		private const string localToWorldPoseDeprecationMessage = "Using this method after 'await locatable.SetEnabledAsync(true);' is error-prone. OVRTask finishes the execution before OVRCameraRig.Update(), so camera will still use a pose from the previous frame. This results in descrepancy when localizing anchors against the stale camera pose.\nUse an overload with the 'trackingSpaceToWorldSpaceTransform' parameter instead.";

		public Vector3? Position { get; }

		public Quaternion? Rotation { get; }

		public bool IsPositionTracked => Flags.IsPositionTracked();

		public bool IsRotationTracked => Flags.IsOrientationTracked();

		internal TrackingSpacePose(Vector3 position, Quaternion rotation, OVRPlugin.SpaceLocationFlags flags)
		{
			Flags = flags;
			Position = (Flags.IsPositionValid() ? new Vector3?(position) : null);
			Rotation = (Flags.IsOrientationValid() ? new Quaternion?(rotation) : null);
		}

		[Obsolete("Using this method after 'await locatable.SetEnabledAsync(true);' is error-prone. OVRTask finishes the execution before OVRCameraRig.Update(), so camera will still use a pose from the previous frame. This results in descrepancy when localizing anchors against the stale camera pose.\nUse an overload with the 'trackingSpaceToWorldSpaceTransform' parameter instead.")]
		public Vector3? ComputeWorldPosition(Camera camera)
		{
			if (camera == null)
			{
				throw new ArgumentNullException("camera");
			}
			if (!Position.HasValue)
			{
				return null;
			}
			OVRPose identity = OVRPose.identity;
			if (!OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.Head, NodeStatePropertyType.Position, OVRPlugin.Node.Head, OVRPlugin.Step.Render, out identity.position))
			{
				return null;
			}
			if (!OVRNodeStateProperties.GetNodeStatePropertyQuaternion(XRNode.Head, NodeStatePropertyType.Orientation, OVRPlugin.Node.Head, OVRPlugin.Step.Render, out identity.orientation))
			{
				return null;
			}
			identity = identity.Inverse();
			Vector3 point = identity.position + identity.orientation * Position.Value;
			return camera.transform.localToWorldMatrix.MultiplyPoint(point);
		}

		[Obsolete("Using this method after 'await locatable.SetEnabledAsync(true);' is error-prone. OVRTask finishes the execution before OVRCameraRig.Update(), so camera will still use a pose from the previous frame. This results in descrepancy when localizing anchors against the stale camera pose.\nUse an overload with the 'trackingSpaceToWorldSpaceTransform' parameter instead.")]
		public Quaternion? ComputeWorldRotation(Camera camera)
		{
			if (camera == null)
			{
				throw new ArgumentNullException("camera");
			}
			if (!Rotation.HasValue)
			{
				return null;
			}
			if (!OVRNodeStateProperties.GetNodeStatePropertyQuaternion(XRNode.Head, NodeStatePropertyType.Orientation, OVRPlugin.Node.Head, OVRPlugin.Step.Render, out var retQuat))
			{
				return null;
			}
			retQuat = Quaternion.Inverse(retQuat);
			Quaternion quaternion = retQuat * Rotation.Value;
			return camera.transform.rotation * quaternion;
		}

		public Vector3? ComputeWorldPosition(Transform trackingSpaceToWorldSpaceTransform)
		{
			if (trackingSpaceToWorldSpaceTransform == null)
			{
				throw new ArgumentNullException("trackingSpaceToWorldSpaceTransform");
			}
			if (!Position.HasValue)
			{
				return null;
			}
			return trackingSpaceToWorldSpaceTransform.TransformPoint(Position.Value);
		}

		public Quaternion? ComputeWorldRotation(Transform trackingSpaceToWorldSpaceTransform)
		{
			if (trackingSpaceToWorldSpaceTransform == null)
			{
				throw new ArgumentNullException("trackingSpaceToWorldSpaceTransform");
			}
			if (!Rotation.HasValue)
			{
				return null;
			}
			return trackingSpaceToWorldSpaceTransform.rotation * Rotation.Value;
		}
	}

	public struct GetSceneAnchorPosesJob : IJobFor
	{
		[ReadOnly]
		public NativeArray<OVRLocatable> Locatables;

		[WriteOnly]
		public NativeArray<TrackingSpacePose> Poses;

		void IJobFor.Execute(int index)
		{
			OVRLocatable oVRLocatable = Locatables[index];
			Poses[index] = ((!oVRLocatable.IsNull && oVRLocatable.TryGetSceneAnchorPose(out var pose)) ? pose : default(TrackingSpacePose));
		}
	}

	public struct GetSpatialAnchorPosesJob : IJobFor
	{
		[ReadOnly]
		public NativeArray<OVRLocatable> Locatables;

		[WriteOnly]
		public NativeArray<TrackingSpacePose> Poses;

		void IJobFor.Execute(int index)
		{
			OVRLocatable oVRLocatable = Locatables[index];
			Poses[index] = ((!oVRLocatable.IsNull && oVRLocatable.TryGetSpatialAnchorPose(out var pose)) ? pose : default(TrackingSpacePose));
		}
	}

	public struct TransformPosesJob : IJobFor
	{
		public NativeArray<TrackingSpacePose> Poses;

		public Matrix4x4 Transform;

		public Quaternion Rotation;

		void IJobFor.Execute(int index)
		{
			TrackingSpacePose trackingSpacePose = Poses[index];
			ref NativeArray<TrackingSpacePose> poses = ref Poses;
			Vector3 position = (trackingSpacePose.Position.HasValue ? Transform.MultiplyPoint(trackingSpacePose.Position.Value) : Vector3.zero);
			Quaternion rotation = Rotation;
			Quaternion? rotation2 = trackingSpacePose.Rotation;
			poses[index] = new TrackingSpacePose(position, (rotation * rotation2) ?? Quaternion.identity, trackingSpacePose.Flags);
		}
	}

	public struct SetWorldSpaceTransformsJob : IJobParallelForTransform
	{
		[ReadOnly]
		public NativeArray<TrackingSpacePose> Poses;

		void IJobParallelForTransform.Execute(int index, TransformAccess transform)
		{
			TrackingSpacePose trackingSpacePose = Poses[index];
			if (trackingSpacePose.Position.HasValue && trackingSpacePose.Rotation.HasValue)
			{
				transform.SetPositionAndRotation(trackingSpacePose.Position.Value, trackingSpacePose.Rotation.Value);
			}
			else if (trackingSpacePose.Position.HasValue)
			{
				transform.position = trackingSpacePose.Position.Value;
			}
			else if (trackingSpacePose.Rotation.HasValue)
			{
				transform.rotation = trackingSpacePose.Rotation.Value;
			}
		}
	}

	public struct SetLocalSpaceTransformsJob : IJobParallelForTransform
	{
		[ReadOnly]
		public NativeArray<TrackingSpacePose> Poses;

		public void Execute(int index, TransformAccess transform)
		{
			TrackingSpacePose trackingSpacePose = Poses[index];
			if (trackingSpacePose.Position.HasValue && trackingSpacePose.Rotation.HasValue)
			{
				transform.SetLocalPositionAndRotation(trackingSpacePose.Position.Value, trackingSpacePose.Rotation.Value);
			}
			else if (trackingSpacePose.Position.HasValue)
			{
				transform.localPosition = trackingSpacePose.Position.Value;
			}
			else if (trackingSpacePose.Rotation.HasValue)
			{
				transform.localRotation = trackingSpacePose.Rotation.Value;
			}
		}
	}

	private struct CopyPosesJob : IJobFor
	{
		[ReadOnly]
		public NativeArray<TrackingSpacePose> PosesIn;

		[WriteOnly]
		public NativeArray<TrackingSpacePose> PosesOut;

		public void Execute(int index)
		{
			PosesOut[index] = PosesIn[index];
		}
	}

	public static readonly OVRLocatable Null;

	OVRPlugin.SpaceComponentType IOVRAnchorComponent<OVRLocatable>.Type => Type;

	ulong IOVRAnchorComponent<OVRLocatable>.Handle => Handle;

	public bool IsNull => Handle == 0;

	public bool IsEnabled
	{
		get
		{
			bool enabled = default(bool);
			bool changePending = default(bool);
			if (!IsNull && OVRPlugin.GetSpaceComponentStatus(Handle, Type, out enabled, out changePending) && enabled)
			{
				return !changePending;
			}
			return false;
		}
	}

	internal OVRPlugin.SpaceComponentType Type => OVRPlugin.SpaceComponentType.Locatable;

	internal ulong Handle { get; }

	OVRLocatable IOVRAnchorComponent<OVRLocatable>.FromAnchor(OVRAnchor anchor)
	{
		return new OVRLocatable(anchor);
	}

	public OVRTask<bool> SetEnabledAsync(bool enabled, double timeout = 0.0)
	{
		if (!OVRPlugin.GetSpaceComponentStatus(Handle, Type, out var enabled2, out var changePending))
		{
			return OVRTask.FromResult(result: false);
		}
		if (changePending)
		{
			return OVRAnchor.CreateDeferredSpaceComponentStatusTask(Handle, Type, enabled, timeout);
		}
		ulong requestId;
		if (enabled2 != enabled)
		{
			return OVRTask.Build(OVRPlugin.SetSpaceComponentStatus(Handle, Type, enabled, timeout, out requestId), requestId).ToTask(failureValue: false);
		}
		return OVRTask.FromResult(result: true);
	}

	[Obsolete("Use SetEnabledAsync instead.")]
	public OVRTask<bool> SetEnabledSafeAsync(bool enabled, double timeout = 0.0)
	{
		return SetEnabledAsync(enabled, timeout);
	}

	public bool Equals(OVRLocatable other)
	{
		return Handle == other.Handle;
	}

	public static bool operator ==(OVRLocatable lhs, OVRLocatable rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(OVRLocatable lhs, OVRLocatable rhs)
	{
		return !lhs.Equals(rhs);
	}

	public override bool Equals(object obj)
	{
		if (obj is OVRLocatable other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Handle.GetHashCode() * 486187739 + ((int)Type).GetHashCode();
	}

	public override string ToString()
	{
		return $"{Handle}.Locatable";
	}

	private OVRLocatable(OVRAnchor anchor)
	{
		Handle = anchor.Handle;
	}

	public bool TryGetSceneAnchorPose(out TrackingSpacePose pose)
	{
		if (!OVRPlugin.TryLocateSpace(Handle, OVRPlugin.GetTrackingOriginType(), out var pose2, out var locationFlags))
		{
			pose = default(TrackingSpacePose);
			return false;
		}
		Vector3 position = pose2.Position.FromFlippedZVector3f();
		Quaternion rotation = new Quaternion(0f - pose2.Orientation.z, pose2.Orientation.w, 0f - pose2.Orientation.x, pose2.Orientation.y);
		pose = new TrackingSpacePose(position, rotation, locationFlags);
		return true;
	}

	public bool TryGetSpatialAnchorPose(out TrackingSpacePose pose)
	{
		if (!OVRPlugin.TryLocateSpace(Handle, OVRPlugin.GetTrackingOriginType(), out var pose2, out var locationFlags))
		{
			pose = default(TrackingSpacePose);
			return false;
		}
		Vector3 position = pose2.Position.FromFlippedZVector3f();
		Quaternion rotation = pose2.Orientation.FromFlippedZQuatf();
		pose = new TrackingSpacePose(position, rotation, locationFlags);
		return true;
	}

	private static JobHandle ScheduleUpdateTransforms(NativeArray<OVRLocatable> locatables, TransformAccessArray transforms, Transform trackingSpaceToWorldSpaceTransform, NativeArray<TrackingSpacePose> posesOut, JobHandle inputDeps)
	{
		if (transforms.length != locatables.Length)
		{
			throw new InvalidOperationException(string.Format("The length of {0} ({1}) must be equal to the length of {2} ({3}).", "transforms", transforms.length, "locatables", locatables.Length));
		}
		if (posesOut.IsCreated && posesOut.Length != locatables.Length)
		{
			throw new InvalidOperationException(string.Format("If {0} is a valid array ({1}=true), then the length of {2} ({3}) must be equal to the length of {4} ({5}).", "posesOut", "IsCreated", "posesOut", posesOut.Length, "locatables", locatables.Length));
		}
		if (locatables.Length == 0)
		{
			return inputDeps;
		}
		NativeArray<TrackingSpacePose> nativeArray = new NativeArray<TrackingSpacePose>(locatables.Length, Allocator.TempJob);
		GetSceneAnchorPosesJob jobData = default(GetSceneAnchorPosesJob);
		jobData.Locatables = locatables;
		jobData.Poses = nativeArray;
		JobHandle jobHandle = jobData.ScheduleParallel(locatables.Length, 4, inputDeps);
		if (posesOut.IsCreated)
		{
			CopyPosesJob jobData2 = default(CopyPosesJob);
			jobData2.PosesIn = nativeArray;
			jobData2.PosesOut = posesOut;
			jobHandle = jobData2.ScheduleParallel(nativeArray.Length, 4, jobHandle);
		}
		if ((bool)trackingSpaceToWorldSpaceTransform)
		{
			TransformPosesJob jobData3 = default(TransformPosesJob);
			jobData3.Poses = nativeArray;
			jobData3.Rotation = trackingSpaceToWorldSpaceTransform.rotation;
			jobData3.Transform = trackingSpaceToWorldSpaceTransform.localToWorldMatrix;
			jobHandle = jobData3.ScheduleParallel(nativeArray.Length, 4, jobHandle);
			SetWorldSpaceTransformsJob jobData4 = default(SetWorldSpaceTransformsJob);
			jobData4.Poses = nativeArray;
			jobHandle = jobData4.Schedule(transforms, jobHandle);
		}
		else
		{
			SetLocalSpaceTransformsJob jobData5 = default(SetLocalSpaceTransformsJob);
			jobData5.Poses = nativeArray;
			jobHandle = jobData5.Schedule(transforms, jobHandle);
		}
		return nativeArray.Dispose(jobHandle);
	}

	public static void UpdateSceneAnchorTransforms(IEnumerable<KeyValuePair<OVRAnchor, Transform>> anchors, Transform trackingSpaceToWorldSpaceTransform = null, List<TrackingSpacePose> trackingSpacePoses = null)
	{
		if (anchors == null)
		{
			throw new ArgumentNullException("anchors");
		}
		using (OVRNativeList<OVRLocatable> oVRNativeList = OVRNativeList.WithSuggestedCapacityFrom(anchors).AllocateEmpty<OVRLocatable>(Allocator.TempJob))
		{
			using TransformAccessArray transforms = new TransformAccessArray(oVRNativeList.Capacity);
			OVRAnchor key;
			Transform value;
			if (anchors is Dictionary<OVRAnchor, Transform> dictionary)
			{
				foreach (KeyValuePair<OVRAnchor, Transform> item in dictionary)
				{
					item.Deconstruct(out key, out value);
					OVRAnchor anchor2 = key;
					Transform transform = value;
					oVRNativeList.Add(GetLocatableOrDefault(anchor2));
					transforms.Add(transform);
				}
			}
			else
			{
				foreach (KeyValuePair<OVRAnchor, Transform> item2 in anchors.ToNonAlloc())
				{
					item2.Deconstruct(out key, out value);
					OVRAnchor anchor3 = key;
					Transform transform2 = value;
					oVRNativeList.Add(GetLocatableOrDefault(anchor3));
					transforms.Add(transform2);
				}
			}
			using NativeArray<TrackingSpacePose> posesOut = new NativeArray<TrackingSpacePose>(oVRNativeList.Count, Allocator.TempJob);
			ScheduleUpdateTransforms(oVRNativeList.AsNativeArray(), transforms, trackingSpaceToWorldSpaceTransform, posesOut, default(JobHandle)).Complete();
			if (trackingSpacePoses == null)
			{
				return;
			}
			trackingSpacePoses.Clear();
			foreach (TrackingSpacePose item3 in posesOut)
			{
				trackingSpacePoses.Add(item3);
			}
		}
		static OVRLocatable GetLocatableOrDefault(OVRAnchor anchor)
		{
			if (!anchor.TryGetComponent<OVRLocatable>(out var component))
			{
				return default(OVRLocatable);
			}
			return component;
		}
	}
}
