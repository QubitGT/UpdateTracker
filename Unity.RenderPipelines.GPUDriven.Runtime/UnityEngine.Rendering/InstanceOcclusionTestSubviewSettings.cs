using System;

namespace UnityEngine.Rendering;

internal struct InstanceOcclusionTestSubviewSettings
{
	public int testCount;

	public int occluderSubviewIndices;

	public int occluderSubviewMask;

	public int cullingSplitIndices;

	public int cullingSplitMask;

	public static InstanceOcclusionTestSubviewSettings FromSpan(ReadOnlySpan<SubviewOcclusionTest> subviewOcclusionTests)
	{
		InstanceOcclusionTestSubviewSettings result = default(InstanceOcclusionTestSubviewSettings);
		for (int i = 0; i < subviewOcclusionTests.Length; i++)
		{
			SubviewOcclusionTest subviewOcclusionTest = subviewOcclusionTests[i];
			result.occluderSubviewIndices |= subviewOcclusionTest.occluderSubviewIndex << 4 * i;
			result.occluderSubviewMask |= 1 << subviewOcclusionTest.occluderSubviewIndex;
			result.cullingSplitIndices |= subviewOcclusionTest.cullingSplitIndex << 4 * i;
			result.cullingSplitMask |= 1 << subviewOcclusionTest.cullingSplitIndex;
		}
		result.testCount = subviewOcclusionTests.Length;
		return result;
	}
}
