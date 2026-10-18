using System;
using System.Runtime.InteropServices;
using System.Text;

namespace OVR.OpenVR;

public class CVRSpatialAnchors
{
	private IVRSpatialAnchors FnTable;

	internal CVRSpatialAnchors(IntPtr pInterface)
	{
		FnTable = (IVRSpatialAnchors)Marshal.PtrToStructure(pInterface, typeof(IVRSpatialAnchors));
	}

	public EVRSpatialAnchorError CreateSpatialAnchorFromDescriptor(string pchDescriptor, ref uint pHandleOut)
	{
		pHandleOut = 0u;
		return FnTable.CreateSpatialAnchorFromDescriptor(pchDescriptor, ref pHandleOut);
	}

	public EVRSpatialAnchorError CreateSpatialAnchorFromPose(uint unDeviceIndex, ETrackingUniverseOrigin eOrigin, ref SpatialAnchorPose_t pPose, ref uint pHandleOut)
	{
		pHandleOut = 0u;
		return FnTable.CreateSpatialAnchorFromPose(unDeviceIndex, eOrigin, ref pPose, ref pHandleOut);
	}

	public EVRSpatialAnchorError GetSpatialAnchorPose(uint unHandle, ETrackingUniverseOrigin eOrigin, ref SpatialAnchorPose_t pPoseOut)
	{
		return FnTable.GetSpatialAnchorPose(unHandle, eOrigin, ref pPoseOut);
	}

	public EVRSpatialAnchorError GetSpatialAnchorDescriptor(uint unHandle, StringBuilder pchDescriptorOut, ref uint punDescriptorBufferLenInOut)
	{
		punDescriptorBufferLenInOut = 0u;
		return FnTable.GetSpatialAnchorDescriptor(unHandle, pchDescriptorOut, ref punDescriptorBufferLenInOut);
	}
}
