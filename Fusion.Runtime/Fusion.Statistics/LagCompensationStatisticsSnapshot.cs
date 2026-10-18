using System.Diagnostics;

namespace Fusion.Statistics;

public class LagCompensationStatisticsSnapshot
{
	public double TotalElapsedTime => AdvanceBufferTime + AddOnBufferTime + AddOnBVHTime + UpdateBufferTime + UpdateBVHTime + RefitBVHTime;

	public int BVHMaxDeep { get; private set; }

	public int BVHNodesCount { get; private set; }

	public int HitboxesCount { get; private set; }

	public double AddOnBufferTime { get; private set; }

	public double AddOnBVHTime { get; private set; }

	public double UpdateBVHTime { get; private set; }

	public double UpdateBufferTime { get; private set; }

	public double AdvanceBufferTime { get; private set; }

	public double RefitBVHTime { get; private set; }

	internal void CopyFromSnapshot(LagCompensationStatisticsSnapshot snapshot)
	{
		BVHMaxDeep = snapshot.BVHMaxDeep;
		BVHNodesCount = snapshot.BVHNodesCount;
		HitboxesCount = snapshot.HitboxesCount;
		AddOnBufferTime = snapshot.AddOnBufferTime;
		AddOnBVHTime = snapshot.AddOnBVHTime;
		UpdateBVHTime = snapshot.UpdateBVHTime;
		UpdateBufferTime = snapshot.UpdateBufferTime;
		AdvanceBufferTime = snapshot.AdvanceBufferTime;
		RefitBVHTime = snapshot.RefitBVHTime;
	}

	internal void ClearSnapshot()
	{
		BVHMaxDeep = 0;
		BVHNodesCount = 0;
		HitboxesCount = 0;
		AddOnBufferTime = 0.0;
		AddOnBVHTime = 0.0;
		UpdateBVHTime = 0.0;
		UpdateBufferTime = 0.0;
		AdvanceBufferTime = 0.0;
		RefitBVHTime = 0.0;
	}

	[Conditional("DEBUG")]
	internal void SetBVHMaxDeep(int value, bool overrideValue = false)
	{
		BVHMaxDeep = (overrideValue ? value : (BVHMaxDeep + value));
	}

	[Conditional("DEBUG")]
	internal void SetBVHNodeCount(int value, bool overrideValue = false)
	{
		BVHNodesCount = (overrideValue ? value : (BVHMaxDeep + value));
	}

	[Conditional("DEBUG")]
	internal void SetHitboxesCount(int value, bool overrideValue = false)
	{
		HitboxesCount = (overrideValue ? value : (HitboxesCount + value));
	}

	[Conditional("DEBUG")]
	internal void SetAddOnBufferTime(double value, bool overrideValue = false)
	{
		AddOnBufferTime = (overrideValue ? value : (AddOnBufferTime + value));
	}

	[Conditional("DEBUG")]
	internal void SetAddOnBVHTime(double value, bool overrideValue = false)
	{
		AddOnBVHTime = (overrideValue ? value : (AddOnBVHTime + value));
	}

	[Conditional("DEBUG")]
	internal void SetUpdateBVHTime(double value, bool overrideValue = false)
	{
		UpdateBVHTime = (overrideValue ? value : (UpdateBVHTime + value));
	}

	[Conditional("DEBUG")]
	internal void SetUpdateBufferTime(double value, bool overrideValue = false)
	{
		UpdateBufferTime = (overrideValue ? value : (UpdateBufferTime + value));
	}

	[Conditional("DEBUG")]
	internal void SetAdvanceBufferTime(double value, bool overrideValue = false)
	{
		AdvanceBufferTime = (overrideValue ? value : (AdvanceBufferTime + value));
	}

	[Conditional("DEBUG")]
	internal void SetRefitBVHTime(double value, bool overrideValue = false)
	{
		RefitBVHTime = (overrideValue ? value : (RefitBVHTime + value));
	}
}
