using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.XR.MRUtilityKit;

public static class AnchorPrefabSpawnerUtilities
{
	public static Matrix4x4 GetTransformationMatrixMatchingAnchorVolume(MRUKAnchor anchorInfo, bool matchAspectRatio, bool calculateFacingDirection, Bounds? prefabBounds, AnchorPrefabSpawner.ScalingMode scalingMode = AnchorPrefabSpawner.ScalingMode.Stretch, AnchorPrefabSpawner.AlignMode alignMode = AnchorPrefabSpawner.AlignMode.Automatic)
	{
		int cardinalAxisIndex;
		Vector3 prefabScaleBasedOnAnchorVolume = GetPrefabScaleBasedOnAnchorVolume(anchorInfo, matchAspectRatio, calculateFacingDirection, prefabBounds, out cardinalAxisIndex, scalingMode);
		Pose poseBasedOnAnchorVolume = GetPoseBasedOnAnchorVolume(anchorInfo, prefabBounds, cardinalAxisIndex, prefabScaleBasedOnAnchorVolume, alignMode);
		return Matrix4x4.TRS(poseBasedOnAnchorVolume.position, poseBasedOnAnchorVolume.rotation, prefabScaleBasedOnAnchorVolume);
	}

	public static Vector3 ScalePrefab(Vector3 localScale, AnchorPrefabSpawner.ScalingMode scalingMode = AnchorPrefabSpawner.ScalingMode.Stretch)
	{
		switch (scalingMode)
		{
		case AnchorPrefabSpawner.ScalingMode.UniformScaling:
		{
			float z = Mathf.Min((localScale.x > localScale.y) ? localScale.y : localScale.x, localScale.z);
			localScale.x = (localScale.y = (localScale.z = z));
			return localScale;
		}
		case AnchorPrefabSpawner.ScalingMode.UniformXZScale:
			localScale.x = (localScale.z = Mathf.Min(localScale.x, localScale.z));
			return localScale;
		case AnchorPrefabSpawner.ScalingMode.NoScaling:
			return Vector3.one;
		case AnchorPrefabSpawner.ScalingMode.Stretch:
			return localScale;
		case AnchorPrefabSpawner.ScalingMode.Custom:
			throw new ArgumentException("A custom scaling method was selected but no implementation was provided. To customize the scaling logic either extend the AnchorPrefabSpawner class or use the defaultscaling mode and modify the prefab's local scale afterwards.");
		default:
			throw new ArgumentOutOfRangeException("scalingMode", scalingMode, "The ScalingMode used is not defined");
		}
	}

	public static Vector3 AlignPrefabPivot(Bounds anchorVolumeBounds, Bounds? prefabBounds, Vector3 localScale, AnchorPrefabSpawner.AlignMode alignMode = AnchorPrefabSpawner.AlignMode.Automatic)
	{
		(Vector3, Vector3) tuple = (default(Vector3), default(Vector3));
		switch (alignMode)
		{
		case AnchorPrefabSpawner.AlignMode.Automatic:
		case AnchorPrefabSpawner.AlignMode.Bottom:
			if (prefabBounds.HasValue)
			{
				Vector3 center2 = prefabBounds.Value.center;
				Vector3 min = prefabBounds.Value.min;
				tuple.Item1 = new Vector3(0f - center2.x, center2.z, min.y);
			}
			tuple.Item2 = anchorVolumeBounds.center;
			tuple.Item2.z = anchorVolumeBounds.min.z;
			break;
		case AnchorPrefabSpawner.AlignMode.Center:
			if (prefabBounds.HasValue)
			{
				Vector3 center = prefabBounds.Value.center;
				tuple.Item1 = new Vector3(0f - center.x, center.z, center.y);
			}
			tuple.Item2 = anchorVolumeBounds.center;
			break;
		case AnchorPrefabSpawner.AlignMode.Custom:
			throw new ArgumentException("A custom volume alignment method was selected but no implementation was provided. To customize the alignment logic either extend the AnchorPrefabSpawner class or use the default alignment mode and modify the prefab's local position afterwards.");
		default:
			throw new ArgumentOutOfRangeException("alignMode", alignMode, "The AlignMode used is not defined");
		case AnchorPrefabSpawner.AlignMode.NoAlignment:
			break;
		}
		tuple.Item1.x *= localScale.x;
		tuple.Item1.y *= localScale.z;
		tuple.Item1.z *= localScale.y;
		return tuple.Item2 - tuple.Item1;
	}

	public static bool GetPrefabWithClosestSizeToAnchor(MRUKAnchor anchor, List<GameObject> prefabList, out GameObject sizeMatchingPrefab)
	{
		sizeMatchingPrefab = null;
		if (!anchor.VolumeBounds.HasValue)
		{
			throw new InvalidOperationException("Cannot match a prefab with the closest size to this anchor as the latter has no volume");
		}
		float num = MathF.Pow(anchor.VolumeBounds.Value.size.x * anchor.VolumeBounds.Value.size.y * anchor.VolumeBounds.Value.size.z, 1f / 3f);
		float num2 = float.PositiveInfinity;
		foreach (GameObject prefab in prefabList)
		{
			Bounds? prefabBounds = Utilities.GetPrefabBounds(prefab);
			if (prefabBounds.HasValue)
			{
				float num3 = Mathf.Pow(prefabBounds.Value.size.x * prefabBounds.Value.size.y * prefabBounds.Value.size.z, 1f / 3f);
				float num4 = Mathf.Abs(num - num3);
				if (!(num4 >= num2))
				{
					num2 = num4;
					sizeMatchingPrefab = prefab;
					return true;
				}
			}
		}
		return false;
	}

	public static Matrix4x4 GetTransformationMatrixMatchingAnchorPlaneRect(MRUKAnchor anchorInfo, Bounds? prefabBounds, AnchorPrefabSpawner.ScalingMode scaling = AnchorPrefabSpawner.ScalingMode.Stretch, AnchorPrefabSpawner.AlignMode alignment = AnchorPrefabSpawner.AlignMode.Automatic)
	{
		Vector3 prefabScaleBasedOnAnchorPlaneRect = GetPrefabScaleBasedOnAnchorPlaneRect(anchorInfo, prefabBounds, scaling);
		Pose poseBasedOnAnchorPlaneRect = GetPoseBasedOnAnchorPlaneRect(anchorInfo, alignment, prefabBounds, prefabScaleBasedOnAnchorPlaneRect);
		return Matrix4x4.TRS(poseBasedOnAnchorPlaneRect.position, poseBasedOnAnchorPlaneRect.rotation, prefabScaleBasedOnAnchorPlaneRect);
	}

	public static GameObject SelectPrefab(MRUKAnchor anchor, AnchorPrefabSpawner.SelectionMode prefabSelectionMode, List<GameObject> prefabs, System.Random random)
	{
		if (prefabs == null || prefabs.Count == 0)
		{
			return null;
		}
		GameObject sizeMatchingPrefab = null;
		switch (prefabSelectionMode)
		{
		case AnchorPrefabSpawner.SelectionMode.Random:
		{
			if (random == null)
			{
				throw new InvalidOperationException("When setting the SelectionMode to random, make sure to call AnchorPrefabSpawnerUtilities.InitializeRandom(seed)");
			}
			int index = random.Next(0, prefabs.Count);
			sizeMatchingPrefab = prefabs[index];
			break;
		}
		case AnchorPrefabSpawner.SelectionMode.ClosestSize:
			GetPrefabWithClosestSizeToAnchor(anchor, prefabs, out sizeMatchingPrefab);
			break;
		case AnchorPrefabSpawner.SelectionMode.Custom:
			throw new ArgumentException("A custom prefab selection method was selected but no implementation was provided. To customize the selection logic extend the AnchorPrefabSpawner class.");
		default:
			throw new ArgumentOutOfRangeException("prefabSelectionMode", prefabSelectionMode, "The SelectionMode used is not defined");
		}
		return sizeMatchingPrefab;
	}

	public static Vector3 AlignPrefabPivot(Rect planeRect, Bounds? prefabBounds, Vector2 localScale, AnchorPrefabSpawner.AlignMode alignMode = AnchorPrefabSpawner.AlignMode.Automatic)
	{
		(Vector3, Vector3) tuple = (default(Vector3), default(Vector3));
		switch (alignMode)
		{
		case AnchorPrefabSpawner.AlignMode.Automatic:
		case AnchorPrefabSpawner.AlignMode.Center:
			tuple.Item1 = prefabBounds?.center ?? Vector3.zero;
			tuple.Item2 = planeRect.center;
			break;
		case AnchorPrefabSpawner.AlignMode.Bottom:
			if (prefabBounds.HasValue)
			{
				Vector3 center = prefabBounds.Value.center;
				Vector3 min = prefabBounds.Value.min;
				tuple.Item1 = new Vector3(center.x, min.y);
			}
			tuple.Item2 = planeRect.center;
			tuple.Item2.y = planeRect.min.y;
			break;
		case AnchorPrefabSpawner.AlignMode.Custom:
			throw new ArgumentException("A custom volume alignment method was selected but no implementation was provided.To customize the alignment logic either extend the AnchorPrefabSpawner class or use the defaultalignment mode and modify the prefab's local position afterwards.");
		default:
			throw new ArgumentOutOfRangeException("alignMode", alignMode, "The AlignMode used is not defined");
		case AnchorPrefabSpawner.AlignMode.NoAlignment:
			break;
		}
		tuple.Item1.Scale(localScale);
		return new Vector3(tuple.Item2.x - tuple.Item1.x, tuple.Item2.y - tuple.Item1.y, 0f);
	}

	public static Vector3 ScalePrefab(Vector2 localScale, AnchorPrefabSpawner.ScalingMode scalingMode = AnchorPrefabSpawner.ScalingMode.Stretch)
	{
		switch (scalingMode)
		{
		case AnchorPrefabSpawner.ScalingMode.UniformScaling:
		case AnchorPrefabSpawner.ScalingMode.UniformXZScale:
			localScale.x = (localScale.y = Mathf.Min(localScale.x, localScale.y));
			break;
		case AnchorPrefabSpawner.ScalingMode.NoScaling:
			localScale = Vector2.one;
			break;
		case AnchorPrefabSpawner.ScalingMode.Custom:
			throw new ArgumentException("A custom scaling method was selected but no implementation was provided. To customize the scaling logic either extend the AnchorPrefabSpawner class or use the defaultscaling mode and modify the prefab's local scale afterwards.");
		default:
			throw new ArgumentOutOfRangeException("scalingMode", scalingMode, null);
		case AnchorPrefabSpawner.ScalingMode.Stretch:
			break;
		}
		return new Vector3(localScale.x, localScale.y, 0.5f * (localScale.x + localScale.y));
	}

	internal static Vector3 GetPrefabScaleBasedOnAnchorVolume(MRUKAnchor anchorInfo, bool matchAspectRatio, bool calculateFacingDirection, Bounds? prefabBounds, out int cardinalAxisIndex, AnchorPrefabSpawner.ScalingMode scaling = AnchorPrefabSpawner.ScalingMode.Stretch)
	{
		cardinalAxisIndex = 0;
		if (!anchorInfo.VolumeBounds.HasValue)
		{
			throw new InvalidOperationException("The prefab's pose can't be calculated when the anchor's volume bounds are null.Consider using GetPrefabScaleBasedOnAnchorPlaneRect in case the anchor has a volume.");
		}
		Vector3 prefabSize = prefabBounds?.size ?? Vector3.one;
		cardinalAxisIndex = 0;
		if (calculateFacingDirection && !matchAspectRatio)
		{
			anchorInfo.Room.GetDirectionAwayFromClosestWall(anchorInfo, out cardinalAxisIndex);
		}
		Bounds volumeBounds = RotateVolumeBounds(anchorInfo.VolumeBounds.Value, cardinalAxisIndex);
		Vector3 size = volumeBounds.size;
		Vector3 localScale = new Vector3(size.x / prefabSize.x, size.z / prefabSize.y, size.y / prefabSize.z);
		if (matchAspectRatio)
		{
			MatchAspectRatio(anchorInfo, calculateFacingDirection, prefabSize, size, ref cardinalAxisIndex, ref volumeBounds, ref localScale);
		}
		return ScalePrefab(localScale, scaling);
	}

	internal static Pose GetPoseBasedOnAnchorVolume(MRUKAnchor anchorInfo, Bounds? prefabBounds, int cardinalAxisIndex, Vector3 localScale, AnchorPrefabSpawner.AlignMode alignment = AnchorPrefabSpawner.AlignMode.Automatic)
	{
		if (!anchorInfo.VolumeBounds.HasValue)
		{
			throw new InvalidOperationException("The prefab's pose can't be calculated when the anchor's volume bounds are null. Consider using GetPoseBasedOnAnchorPlaneRect in case the anchor has a plane rect.");
		}
		Vector3 position = AlignPrefabPivot(RotateVolumeBounds(anchorInfo.VolumeBounds.Value, cardinalAxisIndex), prefabBounds, localScale, alignment);
		Quaternion rotation = Quaternion.Euler((cardinalAxisIndex - 1) * 90, -90f, -90f);
		return new Pose(position, rotation);
	}

	internal static void MatchAspectRatio(MRUKAnchor anchorInfo, bool calculateFacingDirection, Vector3 prefabSize, Vector3 volumeSize, ref int cardinalAxisIndex, ref Bounds volumeBounds, ref Vector3 localScale)
	{
		Vector3 vector = new Vector3(prefabSize.z, prefabSize.y, prefabSize.x);
		Vector3 vector2 = new Vector3(volumeSize.x / vector.x, volumeSize.z / vector.y, volumeSize.y / vector.z);
		float num = Mathf.Max(localScale.x, localScale.z) / Mathf.Min(localScale.x, localScale.z);
		float num2 = Mathf.Max(vector2.x, vector2.z) / Mathf.Min(vector2.x, vector2.z);
		bool flag = num > num2;
		if (flag)
		{
			cardinalAxisIndex = 1;
		}
		if (calculateFacingDirection)
		{
			anchorInfo.Room.GetDirectionAwayFromClosestWall(anchorInfo, out cardinalAxisIndex, flag ? new List<int> { 0, 2 } : new List<int> { 1, 3 });
		}
		if (cardinalAxisIndex != 0 && anchorInfo.VolumeBounds.HasValue)
		{
			volumeBounds = RotateVolumeBounds(anchorInfo.VolumeBounds.Value, cardinalAxisIndex);
			volumeSize = volumeBounds.size;
			localScale = new Vector3(volumeSize.x / prefabSize.x, volumeSize.z / prefabSize.y, volumeSize.y / prefabSize.z);
		}
	}

	internal static Bounds RotateVolumeBounds(Bounds bounds, int rotation)
	{
		Vector3 center = bounds.center;
		Vector3 size = bounds.size;
		return rotation switch
		{
			1 => new Bounds(new Vector3(0f - center.y, center.x, center.z), new Vector3(size.y, size.x, size.z)), 
			2 => new Bounds(new Vector3(0f - center.x, 0f - center.x, center.z), size), 
			3 => new Bounds(new Vector3(center.y, 0f - center.x, center.z), new Vector3(size.y, size.x, size.z)), 
			_ => bounds, 
		};
	}

	internal static Vector3 GetPrefabScaleBasedOnAnchorPlaneRect(MRUKAnchor anchorInfo, Bounds? prefabBounds, AnchorPrefabSpawner.ScalingMode scalingMode = AnchorPrefabSpawner.ScalingMode.Stretch)
	{
		if (!anchorInfo.PlaneRect.HasValue)
		{
			throw new InvalidOperationException("The prefab's pose can't be calculated when the anchor's plane rect is null. Consider using GetPrefabScaleBasedOnAnchorVolume in case the anchor has a volume.");
		}
		Vector3 vector = prefabBounds?.size ?? Vector3.one;
		Vector2 size = anchorInfo.PlaneRect.Value.size;
		return (Vector2)ScalePrefab(new Vector2(size.x / vector.x, size.y / vector.y), scalingMode);
	}

	internal static Pose GetPoseBasedOnAnchorPlaneRect(MRUKAnchor anchorInfo, AnchorPrefabSpawner.AlignMode alignmentMode, Bounds? prefabBounds, Vector2 localScale)
	{
		if (!anchorInfo.PlaneRect.HasValue)
		{
			throw new InvalidOperationException("The prefab's pose can't be calculated when the anchor's plane rect is null. Consider using GetPoseBasedOnAnchorVolume in case the anchor has a volume.");
		}
		Vector3 position = AlignPrefabPivot(anchorInfo.PlaneRect.Value, prefabBounds, localScale, alignmentMode);
		Quaternion identity = Quaternion.identity;
		return new Pose(position, identity);
	}
}
