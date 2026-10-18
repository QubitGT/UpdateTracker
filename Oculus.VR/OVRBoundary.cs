using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[HelpURL("https://developer.oculus.com/documentation/unity/unity-ovrboundary/")]
public class OVRBoundary
{
	public enum Node
	{
		HandLeft = 3,
		HandRight = 4,
		Head = 9
	}

	public enum BoundaryType
	{
		[Obsolete("Deprecated. This enum value will not be supported in OpenXR", false)]
		OuterBoundary = 1,
		PlayArea = 0x100
	}

	[Obsolete("Deprecated. This struct will not be supported in OpenXR", false)]
	public struct BoundaryTestResult
	{
		public bool IsTriggering;

		public float ClosestDistance;

		public Vector3 ClosestPoint;

		public Vector3 ClosestPointNormal;
	}

	private static int cachedVector3fSize = Marshal.SizeOf(typeof(OVRPlugin.Vector3f));

	private static OVRNativeBuffer cachedGeometryNativeBuffer = new OVRNativeBuffer(0);

	private static float[] cachedGeometryManagedBuffer = new float[0];

	private List<Vector3> cachedGeometryList = new List<Vector3>();

	public bool GetConfigured()
	{
		if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
		{
			return OVRPlugin.GetBoundaryConfigured();
		}
		return false;
	}

	[Obsolete("Deprecated. This function will not be supported in OpenXR", false)]
	public BoundaryTestResult TestNode(Node node, BoundaryType boundaryType)
	{
		OVRPlugin.BoundaryTestResult boundaryTestResult = OVRPlugin.TestBoundaryNode((OVRPlugin.Node)node, (OVRPlugin.BoundaryType)boundaryType);
		BoundaryTestResult result = default(BoundaryTestResult);
		result.IsTriggering = boundaryTestResult.IsTriggering == OVRPlugin.Bool.True;
		result.ClosestDistance = boundaryTestResult.ClosestDistance;
		result.ClosestPoint = boundaryTestResult.ClosestPoint.FromFlippedZVector3f();
		result.ClosestPointNormal = boundaryTestResult.ClosestPointNormal.FromFlippedZVector3f();
		return result;
	}

	[Obsolete("Deprecated. This function will not be supported in OpenXR", false)]
	public BoundaryTestResult TestPoint(Vector3 point, BoundaryType boundaryType)
	{
		OVRPlugin.BoundaryTestResult boundaryTestResult = OVRPlugin.TestBoundaryPoint(point.ToFlippedZVector3f(), (OVRPlugin.BoundaryType)boundaryType);
		BoundaryTestResult result = default(BoundaryTestResult);
		result.IsTriggering = boundaryTestResult.IsTriggering == OVRPlugin.Bool.True;
		result.ClosestDistance = boundaryTestResult.ClosestDistance;
		result.ClosestPoint = boundaryTestResult.ClosestPoint.FromFlippedZVector3f();
		result.ClosestPointNormal = boundaryTestResult.ClosestPointNormal.FromFlippedZVector3f();
		return result;
	}

	public Vector3[] GetGeometry(BoundaryType boundaryType)
	{
		if (OVRManager.loadedXRDevice != OVRManager.XRDevice.Oculus)
		{
			Debug.LogError("This functionality is not supported in your current version of Unity.");
			return null;
		}
		int pointsCount = 0;
		if (OVRPlugin.GetBoundaryGeometry2((OVRPlugin.BoundaryType)boundaryType, IntPtr.Zero, ref pointsCount) && pointsCount > 0)
		{
			int num = pointsCount * cachedVector3fSize;
			if (cachedGeometryNativeBuffer.GetCapacity() < num)
			{
				cachedGeometryNativeBuffer.Reset(num);
			}
			int num2 = pointsCount * 3;
			if (cachedGeometryManagedBuffer.Length < num2)
			{
				cachedGeometryManagedBuffer = new float[num2];
			}
			if (OVRPlugin.GetBoundaryGeometry2((OVRPlugin.BoundaryType)boundaryType, cachedGeometryNativeBuffer.GetPointer(), ref pointsCount))
			{
				Marshal.Copy(cachedGeometryNativeBuffer.GetPointer(), cachedGeometryManagedBuffer, 0, num2);
				Vector3[] array = new Vector3[pointsCount];
				for (int i = 0; i < pointsCount; i++)
				{
					array[i] = new OVRPlugin.Vector3f
					{
						x = cachedGeometryManagedBuffer[3 * i],
						y = cachedGeometryManagedBuffer[3 * i + 1],
						z = cachedGeometryManagedBuffer[3 * i + 2]
					}.FromFlippedZVector3f();
				}
				return array;
			}
		}
		return new Vector3[0];
	}

	public Vector3 GetDimensions(BoundaryType boundaryType)
	{
		if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
		{
			return OVRPlugin.GetBoundaryDimensions((OVRPlugin.BoundaryType)boundaryType).FromVector3f();
		}
		return Vector3.zero;
	}

	[Obsolete("Deprecated. This function will not be supported in OpenXR", false)]
	public bool GetVisible()
	{
		if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
		{
			return OVRPlugin.GetBoundaryVisible();
		}
		return false;
	}

	[Obsolete("Deprecated. This function will not be supported in OpenXR", false)]
	public void SetVisible(bool value)
	{
		if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
		{
			OVRPlugin.SetBoundaryVisible(value);
		}
	}
}
