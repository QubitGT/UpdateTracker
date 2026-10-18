using System;

public static class OVREnumExtensions
{
	public static bool IsHand(this OVRSkeleton.SkeletonType skeletonType)
	{
		switch (skeletonType)
		{
		case OVRSkeleton.SkeletonType.HandLeft:
		case OVRSkeleton.SkeletonType.HandRight:
			return true;
		case OVRSkeleton.SkeletonType.XRHandLeft:
		case OVRSkeleton.SkeletonType.XRHandRight:
			return true;
		default:
			return false;
		}
	}

	public static bool IsOpenXRHandSkeleton(this OVRSkeleton.SkeletonType skeletonType)
	{
		if (skeletonType != OVRSkeleton.SkeletonType.XRHandLeft)
		{
			return skeletonType == OVRSkeleton.SkeletonType.XRHandRight;
		}
		return true;
	}

	public static bool IsOVRHandSkeleton(this OVRSkeleton.SkeletonType skeletonType)
	{
		if (skeletonType != 0)
		{
			return skeletonType == OVRSkeleton.SkeletonType.HandRight;
		}
		return true;
	}

	public static bool IsLeft(this OVRSkeleton.SkeletonType type)
	{
		return type switch
		{
			OVRSkeleton.SkeletonType.HandLeft => true, 
			OVRSkeleton.SkeletonType.XRHandLeft => true, 
			_ => false, 
		};
	}

	public static OVRHand.Hand AsHandType(this OVRSkeleton.SkeletonType skeletonType)
	{
		switch (skeletonType)
		{
		case OVRSkeleton.SkeletonType.HandLeft:
		case OVRSkeleton.SkeletonType.XRHandLeft:
			return OVRHand.Hand.HandLeft;
		case OVRSkeleton.SkeletonType.HandRight:
		case OVRSkeleton.SkeletonType.XRHandRight:
			return OVRHand.Hand.HandRight;
		default:
			return OVRHand.Hand.None;
		}
	}

	[Obsolete("Use the overload which takes an OVRHandSkeletonVersioninstead.")]
	public static OVRSkeleton.SkeletonType AsSkeletonType(this OVRHand.Hand hand)
	{
		return hand switch
		{
			OVRHand.Hand.HandLeft => OVRSkeleton.SkeletonType.HandLeft, 
			OVRHand.Hand.HandRight => OVRSkeleton.SkeletonType.HandRight, 
			_ => OVRSkeleton.SkeletonType.None, 
		};
	}

	public static OVRSkeleton.SkeletonType AsSkeletonType(this OVRHand.Hand hand, OVRHandSkeletonVersion version)
	{
		switch (hand)
		{
		case OVRHand.Hand.HandLeft:
			if (version != 0)
			{
				return OVRSkeleton.SkeletonType.XRHandLeft;
			}
			return OVRSkeleton.SkeletonType.HandLeft;
		case OVRHand.Hand.HandRight:
			if (version != 0)
			{
				return OVRSkeleton.SkeletonType.XRHandRight;
			}
			return OVRSkeleton.SkeletonType.HandRight;
		default:
			return OVRSkeleton.SkeletonType.None;
		}
	}

	[Obsolete("Use the overload which takes an OVRHandSkeletonVersioninstead.")]
	public static OVRMesh.MeshType AsMeshType(this OVRHand.Hand hand)
	{
		return hand switch
		{
			OVRHand.Hand.HandLeft => OVRMesh.MeshType.HandLeft, 
			OVRHand.Hand.HandRight => OVRMesh.MeshType.HandRight, 
			_ => OVRMesh.MeshType.None, 
		};
	}

	public static bool IsOpenXRHandMesh(this OVRMesh.MeshType meshType)
	{
		if (meshType != OVRMesh.MeshType.XRHandLeft)
		{
			return meshType == OVRMesh.MeshType.XRHandRight;
		}
		return true;
	}

	public static bool IsOVRHandMesh(this OVRMesh.MeshType meshType)
	{
		if (meshType != 0)
		{
			return meshType == OVRMesh.MeshType.HandRight;
		}
		return true;
	}

	public static OVRMesh.MeshType AsMeshType(this OVRHand.Hand hand, OVRHandSkeletonVersion version)
	{
		switch (hand)
		{
		case OVRHand.Hand.HandLeft:
			if (version != 0)
			{
				return OVRMesh.MeshType.XRHandLeft;
			}
			return OVRMesh.MeshType.HandLeft;
		case OVRHand.Hand.HandRight:
			if (version != 0)
			{
				return OVRMesh.MeshType.XRHandRight;
			}
			return OVRMesh.MeshType.HandRight;
		default:
			return OVRMesh.MeshType.None;
		}
	}

	public static bool IsLeft(this OVRMesh.MeshType type)
	{
		return type switch
		{
			OVRMesh.MeshType.HandLeft => true, 
			OVRMesh.MeshType.XRHandLeft => true, 
			_ => false, 
		};
	}

	public static bool IsHand(this OVRMesh.MeshType meshType)
	{
		switch (meshType)
		{
		case OVRMesh.MeshType.HandLeft:
		case OVRMesh.MeshType.HandRight:
			return true;
		case OVRMesh.MeshType.XRHandLeft:
		case OVRMesh.MeshType.XRHandRight:
			return true;
		default:
			return false;
		}
	}

	public static OVRHand.Hand AsHandType(this OVRMesh.MeshType meshType)
	{
		switch (meshType)
		{
		case OVRMesh.MeshType.HandLeft:
		case OVRMesh.MeshType.XRHandLeft:
			return OVRHand.Hand.HandLeft;
		case OVRMesh.MeshType.HandRight:
		case OVRMesh.MeshType.XRHandRight:
			return OVRHand.Hand.HandRight;
		default:
			return OVRHand.Hand.None;
		}
	}
}
