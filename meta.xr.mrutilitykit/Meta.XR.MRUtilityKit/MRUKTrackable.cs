using System;
using System.Collections.Generic;
using System.Text;
using Meta.XR.Util;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Meta.XR.MRUtilityKit;

[Feature(Feature.TrackedKeyboard)]
public sealed class MRUKTrackable : MRUKAnchor
{
	public OVRAnchor.TrackableType TrackableType { get; private set; }

	public bool IsTracked { get; internal set; }

	public string MarkerPayloadString { get; private set; }

	public byte[] MarkerPayloadBytes { get; private set; }

	internal unsafe void OnFetch()
	{
		TrackableType = base.Anchor.GetTrackableType();
		List<OVRPlugin.SpaceComponentType> list;
		using (new OVRObjectPool.ListScope<OVRPlugin.SpaceComponentType>(out list))
		{
			if (!base.Anchor.GetSupportedComponents(list))
			{
				return;
			}
			foreach (OVRPlugin.SpaceComponentType item in list)
			{
				if (!OVRPlugin.GetSpaceComponentStatus(base.Anchor.Handle, item, out var flag, out var _) || !flag)
				{
					continue;
				}
				switch (item)
				{
				case OVRPlugin.SpaceComponentType.MarkerPayload:
				{
					OVRMarkerPayload component3 = base.Anchor.GetComponent<OVRMarkerPayload>();
					using (NativeArray<byte> nativeArray = new NativeArray<byte>(component3.ByteCount, Allocator.Temp))
					{
						Span<byte> span = new Span<byte>(nativeArray.GetUnsafePtr(), nativeArray.Length);
						Span<byte> span2 = span;
						Span<byte> span3 = span2.Slice(0, component3.GetBytes(span));
						if (!span3.SequenceEqual(MarkerPayloadBytes))
						{
							MarkerPayloadBytes = span3.ToArray();
							MarkerPayloadString = ((component3.PayloadType == OVRMarkerPayloadType.StringQRCode) ? Encoding.UTF8.GetString(MarkerPayloadBytes) : null);
						}
					}
					break;
				}
				case OVRPlugin.SpaceComponentType.Bounded2D:
				{
					OVRBounded2D component2 = base.Anchor.GetComponent<OVRBounded2D>();
					base.PlaneRect = component2.BoundingBox;
					base.PlaneBoundary2D = GetUpdatedBoundary(component2, base.PlaneBoundary2D);
					break;
				}
				case OVRPlugin.SpaceComponentType.Bounded3D:
					base.VolumeBounds = base.Anchor.GetComponent<OVRBounded3D>().BoundingBox;
					break;
				}
			}
		}
		static List<Vector2> GetUpdatedBoundary(OVRBounded2D component, List<Vector2> currentBoundary)
		{
			if (component.TryGetBoundaryPointsCount(out var count))
			{
				using NativeArray<Vector2> positions = new NativeArray<Vector2>(count, Allocator.Temp);
				if (component.TryGetBoundaryPoints(positions))
				{
					if (currentBoundary == null)
					{
						currentBoundary = new List<Vector2>(positions.Length);
					}
					currentBoundary.Clear();
					foreach (Vector2 item2 in positions)
					{
						currentBoundary.Add(item2);
					}
					return currentBoundary;
				}
			}
			return null;
		}
	}

	internal void OnInstantiate(OVRAnchor anchor)
	{
		base.Anchor = anchor;
		IsTracked = true;
		OnFetch();
	}
}
