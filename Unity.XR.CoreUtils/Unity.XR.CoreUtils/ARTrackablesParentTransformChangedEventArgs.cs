using System;
using UnityEngine;

namespace Unity.XR.CoreUtils;

public readonly struct ARTrackablesParentTransformChangedEventArgs : IEquatable<ARTrackablesParentTransformChangedEventArgs>
{
	public XROrigin Origin { get; }

	public Transform TrackablesParent { get; }

	public ARTrackablesParentTransformChangedEventArgs(XROrigin origin, Transform trackablesParent)
	{
		if (origin == null)
		{
			throw new ArgumentNullException("origin");
		}
		if (trackablesParent == null)
		{
			throw new ArgumentNullException("trackablesParent");
		}
		Origin = origin;
		TrackablesParent = trackablesParent;
	}

	public bool Equals(ARTrackablesParentTransformChangedEventArgs other)
	{
		if (Origin == other.Origin)
		{
			return TrackablesParent == other.TrackablesParent;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is ARTrackablesParentTransformChangedEventArgs other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCodeUtil.Combine(HashCodeUtil.ReferenceHash(Origin), HashCodeUtil.ReferenceHash(TrackablesParent));
	}

	public static bool operator ==(ARTrackablesParentTransformChangedEventArgs lhs, ARTrackablesParentTransformChangedEventArgs rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(ARTrackablesParentTransformChangedEventArgs lhs, ARTrackablesParentTransformChangedEventArgs rhs)
	{
		return !lhs.Equals(rhs);
	}
}
