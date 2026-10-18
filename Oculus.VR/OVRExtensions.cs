using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using OVR.OpenVR;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR;

public static class OVRExtensions
{
	public static bool IsQRCode(this OVRMarkerPayloadType value)
	{
		return value switch
		{
			OVRMarkerPayloadType.InvalidQRCode => true, 
			OVRMarkerPayloadType.StringQRCode => true, 
			OVRMarkerPayloadType.BinaryQRCode => true, 
			_ => false, 
		};
	}

	public static OVRPose ToTrackingSpacePose(this Transform transform, Camera camera)
	{
		OVRPose identity = OVRPose.identity;
		if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.Head, NodeStatePropertyType.Position, OVRPlugin.Node.Head, OVRPlugin.Step.Render, out var retVec))
		{
			identity.position = retVec;
		}
		if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(XRNode.Head, NodeStatePropertyType.Orientation, OVRPlugin.Node.Head, OVRPlugin.Step.Render, out var retQuat))
		{
			identity.orientation = retQuat;
		}
		return identity * transform.ToHeadSpacePose(camera);
	}

	[Obsolete("ToWorldSpacePose should be invoked with an explicit mainCamera parameter")]
	public static OVRPose ToWorldSpacePose(this OVRPose trackingSpacePose)
	{
		return trackingSpacePose.ToWorldSpacePose(Camera.main);
	}

	public static OVRPose ToWorldSpacePose(this OVRPose trackingSpacePose, Camera mainCamera)
	{
		OVRPose oVRPose = trackingSpacePose.ToHeadSpacePose();
		Matrix4x4 localToWorldMatrix = mainCamera.transform.localToWorldMatrix;
		Matrix4x4 matrix4x = Matrix4x4.TRS(oVRPose.position, oVRPose.orientation, Vector3.one);
		Matrix4x4 matrix4x2 = localToWorldMatrix * matrix4x;
		OVRPose result = default(OVRPose);
		result.position = matrix4x2.GetColumn(3);
		result.orientation = matrix4x2.rotation;
		return result;
	}

	public static OVRPose ToHeadSpacePose(this OVRPose trackingSpacePose)
	{
		OVRPose identity = OVRPose.identity;
		if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.Head, NodeStatePropertyType.Position, OVRPlugin.Node.Head, OVRPlugin.Step.Render, out var retVec))
		{
			identity.position = retVec;
		}
		if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(XRNode.Head, NodeStatePropertyType.Orientation, OVRPlugin.Node.Head, OVRPlugin.Step.Render, out var retQuat))
		{
			identity.orientation = retQuat;
		}
		return identity.Inverse() * trackingSpacePose;
	}

	public static OVRPose ToHeadSpacePose(this Transform transform, Camera camera)
	{
		Quaternion orientation = Quaternion.Inverse(camera.transform.rotation) * transform.rotation;
		Vector4 column = (camera.transform.worldToLocalMatrix * transform.localToWorldMatrix).GetColumn(3);
		OVRPose result = default(OVRPose);
		result.orientation = orientation;
		result.position = column;
		return result;
	}

	public static OVRPose ToOVRPose(this Transform t, bool isLocal = false)
	{
		OVRPose result = default(OVRPose);
		result.orientation = (isLocal ? t.localRotation : t.rotation);
		result.position = (isLocal ? t.localPosition : t.position);
		return result;
	}

	public static void FromOVRPose(this Transform t, OVRPose pose, bool isLocal = false)
	{
		if (isLocal)
		{
			t.localRotation = pose.orientation;
			t.localPosition = pose.position;
		}
		else
		{
			t.rotation = pose.orientation;
			t.position = pose.position;
		}
	}

	public static OVRPose ToOVRPose(this OVRPlugin.Posef p)
	{
		OVRPose result = default(OVRPose);
		result.position = new Vector3(p.Position.x, p.Position.y, 0f - p.Position.z);
		result.orientation = new Quaternion(0f - p.Orientation.x, 0f - p.Orientation.y, p.Orientation.z, p.Orientation.w);
		return result;
	}

	public static OVRTracker.Frustum ToFrustum(this OVRPlugin.Frustumf f)
	{
		OVRTracker.Frustum result = default(OVRTracker.Frustum);
		result.nearZ = f.zNear;
		result.farZ = f.zFar;
		result.fov = new Vector2
		{
			x = 57.29578f * f.fovX,
			y = 57.29578f * f.fovY
		};
		return result;
	}

	public static Color FromColorf(this OVRPlugin.Colorf c)
	{
		Color result = default(Color);
		result.r = c.r;
		result.g = c.g;
		result.b = c.b;
		result.a = c.a;
		return result;
	}

	public static OVRPlugin.Colorf ToColorf(this Color c)
	{
		OVRPlugin.Colorf result = default(OVRPlugin.Colorf);
		result.r = c.r;
		result.g = c.g;
		result.b = c.b;
		result.a = c.a;
		return result;
	}

	public static Vector2 FromSizef(this OVRPlugin.Sizef v)
	{
		Vector2 result = default(Vector2);
		result.x = v.w;
		result.y = v.h;
		return result;
	}

	public static OVRPlugin.Sizef ToSizef(this Vector2 v)
	{
		OVRPlugin.Sizef result = default(OVRPlugin.Sizef);
		result.w = v.x;
		result.h = v.y;
		return result;
	}

	public static Vector2 FromVector2f(this OVRPlugin.Vector2f v)
	{
		Vector2 result = default(Vector2);
		result.x = v.x;
		result.y = v.y;
		return result;
	}

	public static Vector2 FromFlippedXVector2f(this OVRPlugin.Vector2f v)
	{
		Vector2 result = default(Vector2);
		result.x = 0f - v.x;
		result.y = v.y;
		return result;
	}

	public static OVRPlugin.Vector2f ToVector2f(this Vector2 v)
	{
		OVRPlugin.Vector2f result = default(OVRPlugin.Vector2f);
		result.x = v.x;
		result.y = v.y;
		return result;
	}

	public static Vector3 FromSize3f(this OVRPlugin.Size3f v)
	{
		Vector3 result = default(Vector3);
		result.x = v.w;
		result.y = v.h;
		result.z = v.d;
		return result;
	}

	public static OVRPlugin.Size3f ToSize3f(this Vector3 v)
	{
		OVRPlugin.Size3f result = default(OVRPlugin.Size3f);
		result.w = v.x;
		result.h = v.y;
		result.d = v.z;
		return result;
	}

	public static Vector3 FromVector3f(this OVRPlugin.Vector3f v)
	{
		Vector3 result = default(Vector3);
		result.x = v.x;
		result.y = v.y;
		result.z = v.z;
		return result;
	}

	public static Vector3 FromFlippedXVector3f(this OVRPlugin.Vector3f v)
	{
		Vector3 result = default(Vector3);
		result.x = 0f - v.x;
		result.y = v.y;
		result.z = v.z;
		return result;
	}

	public static Vector3 FromFlippedZVector3f(this OVRPlugin.Vector3f v)
	{
		Vector3 result = default(Vector3);
		result.x = v.x;
		result.y = v.y;
		result.z = 0f - v.z;
		return result;
	}

	public static OVRPlugin.Vector3f ToVector3f(this Vector3 v)
	{
		OVRPlugin.Vector3f result = default(OVRPlugin.Vector3f);
		result.x = v.x;
		result.y = v.y;
		result.z = v.z;
		return result;
	}

	public static OVRPlugin.Vector3f ToFlippedXVector3f(this Vector3 v)
	{
		OVRPlugin.Vector3f result = default(OVRPlugin.Vector3f);
		result.x = 0f - v.x;
		result.y = v.y;
		result.z = v.z;
		return result;
	}

	public static OVRPlugin.Vector3f ToFlippedZVector3f(this Vector3 v)
	{
		OVRPlugin.Vector3f result = default(OVRPlugin.Vector3f);
		result.x = v.x;
		result.y = v.y;
		result.z = 0f - v.z;
		return result;
	}

	public static Vector4 FromVector4f(this OVRPlugin.Vector4f v)
	{
		Vector4 result = default(Vector4);
		result.x = v.x;
		result.y = v.y;
		result.z = v.z;
		result.w = v.w;
		return result;
	}

	public static OVRPlugin.Vector4f ToVector4f(this Vector4 v)
	{
		OVRPlugin.Vector4f result = default(OVRPlugin.Vector4f);
		result.x = v.x;
		result.y = v.y;
		result.z = v.z;
		result.w = v.w;
		return result;
	}

	public static Quaternion FromQuatf(this OVRPlugin.Quatf q)
	{
		Quaternion result = default(Quaternion);
		result.x = q.x;
		result.y = q.y;
		result.z = q.z;
		result.w = q.w;
		return result;
	}

	public static Quaternion FromFlippedXQuatf(this OVRPlugin.Quatf q)
	{
		Quaternion result = default(Quaternion);
		result.x = q.x;
		result.y = 0f - q.y;
		result.z = 0f - q.z;
		result.w = q.w;
		return result;
	}

	public static Quaternion FromFlippedZQuatf(this OVRPlugin.Quatf q)
	{
		Quaternion result = default(Quaternion);
		result.x = 0f - q.x;
		result.y = 0f - q.y;
		result.z = q.z;
		result.w = q.w;
		return result;
	}

	public static OVRPlugin.Quatf ToQuatf(this Quaternion q)
	{
		OVRPlugin.Quatf result = default(OVRPlugin.Quatf);
		result.x = q.x;
		result.y = q.y;
		result.z = q.z;
		result.w = q.w;
		return result;
	}

	public static OVRPlugin.Quatf ToFlippedXQuatf(this Quaternion q)
	{
		OVRPlugin.Quatf result = default(OVRPlugin.Quatf);
		result.x = q.x;
		result.y = 0f - q.y;
		result.z = 0f - q.z;
		result.w = q.w;
		return result;
	}

	public static OVRPlugin.Quatf ToFlippedZQuatf(this Quaternion q)
	{
		OVRPlugin.Quatf result = default(OVRPlugin.Quatf);
		result.x = 0f - q.x;
		result.y = 0f - q.y;
		result.z = q.z;
		result.w = q.w;
		return result;
	}

	public static HmdMatrix34_t ConvertToHMDMatrix34(this Matrix4x4 m)
	{
		HmdMatrix34_t result = default(HmdMatrix34_t);
		result.m0 = m[0, 0];
		result.m1 = m[0, 1];
		result.m2 = 0f - m[0, 2];
		result.m3 = m[0, 3];
		result.m4 = m[1, 0];
		result.m5 = m[1, 1];
		result.m6 = 0f - m[1, 2];
		result.m7 = m[1, 3];
		result.m8 = 0f - m[2, 0];
		result.m9 = 0f - m[2, 1];
		result.m10 = m[2, 2];
		result.m11 = 0f - m[2, 3];
		return result;
	}

	public static Transform FindChildRecursive(this Transform parent, string name)
	{
		for (int i = 0; i < parent.childCount; i++)
		{
			Transform child = parent.GetChild(i);
			if (child.name.Contains(name))
			{
				return child;
			}
			Transform transform = child.FindChildRecursive(name);
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}

	public static bool Equals(this Gradient gradient, Gradient otherGradient)
	{
		if (gradient.colorKeys.Length != otherGradient.colorKeys.Length || gradient.alphaKeys.Length != otherGradient.alphaKeys.Length)
		{
			return false;
		}
		for (int i = 0; i < gradient.colorKeys.Length; i++)
		{
			GradientColorKey gradientColorKey = gradient.colorKeys[i];
			GradientColorKey gradientColorKey2 = otherGradient.colorKeys[i];
			if (gradientColorKey.color != gradientColorKey2.color || gradientColorKey.time != gradientColorKey2.time)
			{
				return false;
			}
		}
		for (int j = 0; j < gradient.alphaKeys.Length; j++)
		{
			GradientAlphaKey gradientAlphaKey = gradient.alphaKeys[j];
			GradientAlphaKey gradientAlphaKey2 = otherGradient.alphaKeys[j];
			if (gradientAlphaKey.alpha != gradientAlphaKey2.alpha || gradientAlphaKey.time != gradientAlphaKey2.time)
			{
				return false;
			}
		}
		return true;
	}

	public static void CopyFrom(this Gradient gradient, Gradient otherGradient)
	{
		GradientColorKey[] array = new GradientColorKey[otherGradient.colorKeys.Length];
		for (int i = 0; i < array.Length; i++)
		{
			Color color = otherGradient.colorKeys[i].color;
			array[i].color = new Color(color.r, color.g, color.b, color.a);
			array[i].time = otherGradient.colorKeys[i].time;
		}
		GradientAlphaKey[] array2 = new GradientAlphaKey[otherGradient.alphaKeys.Length];
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j].alpha = otherGradient.alphaKeys[j].alpha;
			array2[j].time = otherGradient.alphaKeys[j].time;
		}
		gradient.SetKeys(array, array2);
	}

	[Obsolete("Anchor APIs that specify a storage location are obsolete.")]
	internal static OVRPlugin.SpaceStorageLocation ToSpaceStorageLocation(this OVRSpace.StorageLocation storageLocation)
	{
		return storageLocation switch
		{
			OVRSpace.StorageLocation.Local => OVRPlugin.SpaceStorageLocation.Local, 
			OVRSpace.StorageLocation.Cloud => OVRPlugin.SpaceStorageLocation.Cloud, 
			_ => throw new NotSupportedException(string.Format("{0} is not a supported {1}", storageLocation, "SpaceStorageLocation")), 
		};
	}

	internal static OVREnumerable<T> ToNonAlloc<T>([NoEnumeration] this IEnumerable<T> enumerable)
	{
		return new OVREnumerable<T>(enumerable);
	}

	internal static NativeArray<T> ToNativeArray<T>(this IEnumerable<T> enumerable, Allocator allocator) where T : struct
	{
		if (enumerable == null)
		{
			throw new ArgumentNullException("enumerable");
		}
		if (!(enumerable is T[] array))
		{
			if (!(enumerable is IReadOnlyList<T> readOnlyList))
			{
				if (!(enumerable is HashSet<T> hashSet))
				{
					if (!(enumerable is Queue<T> queue))
					{
						if (!(enumerable is IReadOnlyCollection<T> readOnlyCollection))
						{
							if (enumerable is ICollection<T> collection)
							{
								NativeArray<T> result = new NativeArray<T>(collection.Count, allocator, NativeArrayOptions.UninitializedMemory);
								int num = 0;
								{
									foreach (T item in collection)
									{
										result[num++] = item;
									}
									return result;
								}
							}
							int num2 = 0;
							int num3 = 4;
							NativeArray<T> nativeArray = new NativeArray<T>(num3, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
							foreach (T item2 in enumerable)
							{
								if (num2 == num3)
								{
									num3 *= 2;
									NativeArray<T> nativeArray2;
									using (nativeArray)
									{
										nativeArray2 = new NativeArray<T>(num3, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
										NativeArray<T>.Copy(nativeArray, nativeArray2, nativeArray.Length);
									}
									nativeArray = nativeArray2;
								}
								nativeArray[num2++] = item2;
							}
							using (nativeArray)
							{
								NativeArray<T> nativeArray4 = new NativeArray<T>(num2, allocator, NativeArrayOptions.UninitializedMemory);
								NativeArray<T>.Copy(nativeArray, nativeArray4, num2);
								return nativeArray4;
							}
						}
						NativeArray<T> result2 = new NativeArray<T>(readOnlyCollection.Count, allocator, NativeArrayOptions.UninitializedMemory);
						int num4 = 0;
						{
							foreach (T item3 in readOnlyCollection)
							{
								result2[num4++] = item3;
							}
							return result2;
						}
					}
					NativeArray<T> result3 = new NativeArray<T>(queue.Count, allocator, NativeArrayOptions.UninitializedMemory);
					int num5 = 0;
					{
						foreach (T item4 in queue)
						{
							result3[num5++] = item4;
						}
						return result3;
					}
				}
				NativeArray<T> result4 = new NativeArray<T>(hashSet.Count, allocator, NativeArrayOptions.UninitializedMemory);
				int num6 = 0;
				{
					foreach (T item5 in hashSet)
					{
						result4[num6++] = item5;
					}
					return result4;
				}
			}
			NativeArray<T> result5 = new NativeArray<T>(readOnlyList.Count, allocator, NativeArrayOptions.UninitializedMemory);
			for (int i = 0; i < result5.Length; i++)
			{
				result5[i] = readOnlyList[i];
			}
			return result5;
		}
		return new NativeArray<T>(array, allocator);
	}
}
