using System;
using UnityEngine;

namespace Meta.XR.MultiplayerBlocks.Shared;

[Serializable]
internal class ColocationDebuggingOptions
{
	[SerializeField]
	[Tooltip("Show the alignment anchor with debug visual, colocated players should be seeing the anchor at the same physical location.")]
	internal bool visualizeAlignmentAnchor = true;

	[SerializeField]
	[Tooltip("Enable verbose logging to debug colocation process")]
	internal bool enableVerboseLogging;
}
