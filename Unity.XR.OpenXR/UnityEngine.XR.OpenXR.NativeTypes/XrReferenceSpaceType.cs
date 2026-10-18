using System;

namespace UnityEngine.XR.OpenXR.NativeTypes;

[Flags]
public enum XrReferenceSpaceType
{
	View = 1,
	Local = 2,
	Stage = 3,
	UnboundedMsft = 0x3B9B5E70,
	CombinedEyeVarjo = 0x3B9CA2A8
}
