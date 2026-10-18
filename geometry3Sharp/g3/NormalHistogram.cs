using System.Collections.Generic;

namespace g3;

public class NormalHistogram
{
	public int Bins = 1024;

	public SphericalFibonacciPointSet Points;

	public double[] Counts;

	public HashSet<int> UsedBins;

	public NormalHistogram(int bins, bool bTrackUsed = false)
	{
		Bins = bins;
		Points = new SphericalFibonacciPointSet(bins);
		Counts = new double[bins];
		if (bTrackUsed)
		{
			UsedBins = new HashSet<int>();
		}
	}

	public NormalHistogram(DMesh3 mesh, bool bWeightByArea = true, int bins = 1024)
		: this(bins)
	{
		CountFaceNormals(mesh, bWeightByArea);
	}

	public void Count(Vector3d pt, double weight = 1.0, bool bIsNormalized = false)
	{
		int num = Points.NearestPoint(pt, bIsNormalized);
		Counts[num] += weight;
		if (UsedBins != null)
		{
			UsedBins.Add(num);
		}
	}

	public void CountFaceNormals(DMesh3 mesh, bool bWeightByArea = true)
	{
		foreach (int item in mesh.TriangleIndices())
		{
			if (bWeightByArea)
			{
				mesh.GetTriInfo(item, out var normal, out var fArea, out var _);
				Count(normal, fArea, bIsNormalized: true);
			}
			else
			{
				Count(mesh.GetTriNormal(item), 1.0, bIsNormalized: true);
			}
		}
	}

	public Vector3d FindMaxNormal()
	{
		int num = 0;
		for (int i = 1; i < Bins; i++)
		{
			if (Counts[i] > Counts[num])
			{
				num = i;
			}
		}
		return Points[num];
	}
}
