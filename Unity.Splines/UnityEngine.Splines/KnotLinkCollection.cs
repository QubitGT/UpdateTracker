using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.Splines;

[Serializable]
public sealed class KnotLinkCollection
{
	[Serializable]
	private sealed class KnotLink : IReadOnlyList<SplineKnotIndex>, IEnumerable<SplineKnotIndex>, IEnumerable, IReadOnlyCollection<SplineKnotIndex>
	{
		public SplineKnotIndex[] Knots;

		public int Count => Knots.Length;

		public SplineKnotIndex this[int index] => Knots[index];

		public IEnumerator<SplineKnotIndex> GetEnumerator()
		{
			return ((IEnumerable<SplineKnotIndex>)Knots).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Knots.GetEnumerator();
		}
	}

	[SerializeField]
	private KnotLink[] m_KnotsLink = new KnotLink[0];

	public int Count => m_KnotsLink.Length;

	private KnotLink GetKnotLinksInternal(SplineKnotIndex index)
	{
		KnotLink[] knotsLink = m_KnotsLink;
		foreach (KnotLink knotLink in knotsLink)
		{
			if (Array.IndexOf(knotLink.Knots, index) >= 0)
			{
				return knotLink;
			}
		}
		return null;
	}

	public bool TryGetKnotLinks(SplineKnotIndex knotIndex, out IReadOnlyList<SplineKnotIndex> linkedKnots)
	{
		linkedKnots = GetKnotLinksInternal(knotIndex);
		return linkedKnots != null;
	}

	public IReadOnlyList<SplineKnotIndex> GetKnotLinks(SplineKnotIndex knotIndex)
	{
		if (TryGetKnotLinks(knotIndex, out var linkedKnots))
		{
			return linkedKnots;
		}
		KnotLink knotLink = new KnotLink();
		knotLink.Knots = new SplineKnotIndex[1] { knotIndex };
		return knotLink;
	}

	public void Clear()
	{
		m_KnotsLink = new KnotLink[0];
	}

	public void Link(SplineKnotIndex knotA, SplineKnotIndex knotB)
	{
		if (knotA.Equals(knotB))
		{
			return;
		}
		KnotLink knotLinksInternal = GetKnotLinksInternal(knotA);
		KnotLink knotLinksInternal2 = GetKnotLinksInternal(knotB);
		if (knotLinksInternal != null && knotLinksInternal2 != null)
		{
			if (!knotLinksInternal.Equals(knotLinksInternal2))
			{
				SplineKnotIndex[] array = new SplineKnotIndex[knotLinksInternal.Knots.Length + knotLinksInternal2.Knots.Length];
				Array.Copy(knotLinksInternal.Knots, array, knotLinksInternal.Knots.Length);
				Array.Copy(knotLinksInternal2.Knots, 0, array, knotLinksInternal.Knots.Length, knotLinksInternal2.Knots.Length);
				knotLinksInternal.Knots = array;
				ArrayUtility.Remove(ref m_KnotsLink, knotLinksInternal2);
			}
		}
		else if (knotLinksInternal2 != null)
		{
			SplineKnotIndex[] array2 = knotLinksInternal2.Knots;
			ArrayUtility.Add(ref array2, knotA);
			knotLinksInternal2.Knots = array2;
		}
		else if (knotLinksInternal != null)
		{
			SplineKnotIndex[] array3 = knotLinksInternal.Knots;
			ArrayUtility.Add(ref array3, knotB);
			knotLinksInternal.Knots = array3;
		}
		else
		{
			KnotLink knotLink = new KnotLink();
			knotLink.Knots = new SplineKnotIndex[2] { knotA, knotB };
			KnotLink element = knotLink;
			ArrayUtility.Add(ref m_KnotsLink, element);
		}
	}

	public void Unlink(SplineKnotIndex knot)
	{
		KnotLink knotLinksInternal = GetKnotLinksInternal(knot);
		if (knotLinksInternal != null)
		{
			SplineKnotIndex[] array = knotLinksInternal.Knots;
			ArrayUtility.Remove(ref array, knot);
			knotLinksInternal.Knots = array;
			if (knotLinksInternal.Knots.Length < 2)
			{
				ArrayUtility.Remove(ref m_KnotsLink, knotLinksInternal);
			}
		}
	}

	public void SplineRemoved(int splineIndex)
	{
		List<int> list = new List<int>(1);
		for (int num = m_KnotsLink.Length - 1; num >= 0; num--)
		{
			KnotLink knotLink = m_KnotsLink[num];
			list.Clear();
			for (int i = 0; i < knotLink.Knots.Length; i++)
			{
				if (knotLink.Knots[i].Spline == splineIndex)
				{
					list.Add(i);
				}
			}
			if (knotLink.Knots.Length - list.Count < 2)
			{
				ArrayUtility.RemoveAt(ref m_KnotsLink, num);
			}
			else
			{
				SplineKnotIndex[] array = knotLink.Knots;
				ArrayUtility.SortedRemoveAt(ref array, list);
				knotLink.Knots = array;
			}
			for (int j = 0; j < knotLink.Knots.Length; j++)
			{
				SplineKnotIndex splineKnotIndex = knotLink.Knots[j];
				if (splineKnotIndex.Spline > splineIndex)
				{
					knotLink.Knots[j] = new SplineKnotIndex(splineKnotIndex.Spline - 1, splineKnotIndex.Knot);
				}
			}
		}
	}

	public void SplineIndexChanged(int previousIndex, int newIndex)
	{
		for (int num = m_KnotsLink.Length - 1; num >= 0; num--)
		{
			KnotLink knotLink = m_KnotsLink[num];
			for (int i = 0; i < knotLink.Knots.Length; i++)
			{
				SplineKnotIndex splineKnotIndex = knotLink.Knots[i];
				if (splineKnotIndex.Spline == previousIndex)
				{
					knotLink.Knots[i] = new SplineKnotIndex(newIndex, splineKnotIndex.Knot);
				}
				else if (splineKnotIndex.Spline > previousIndex && splineKnotIndex.Spline <= newIndex)
				{
					knotLink.Knots[i] = new SplineKnotIndex(splineKnotIndex.Spline - 1, splineKnotIndex.Knot);
				}
				else if (splineKnotIndex.Spline < previousIndex && splineKnotIndex.Spline >= newIndex)
				{
					knotLink.Knots[i] = new SplineKnotIndex(splineKnotIndex.Spline + 1, splineKnotIndex.Knot);
				}
			}
		}
	}

	public void KnotIndexChanged(int splineIndex, int previousKnotIndex, int newKnotIndex)
	{
		KnotIndexChanged(new SplineKnotIndex(splineIndex, previousKnotIndex), new SplineKnotIndex(splineIndex, newKnotIndex));
	}

	public void KnotIndexChanged(SplineKnotIndex previousIndex, SplineKnotIndex newIndex)
	{
		if (previousIndex.Knot > newIndex.Knot)
		{
			previousIndex.Knot++;
		}
		else
		{
			newIndex.Knot++;
		}
		KnotInserted(newIndex);
		Link(previousIndex, newIndex);
		KnotRemoved(previousIndex);
	}

	public void KnotRemoved(int splineIndex, int knotIndex)
	{
		KnotRemoved(new SplineKnotIndex(splineIndex, knotIndex));
	}

	public void KnotRemoved(SplineKnotIndex index)
	{
		Unlink(index);
		ShiftKnotIndices(index, -1);
	}

	public void KnotInserted(int splineIndex, int knotIndex)
	{
		KnotInserted(new SplineKnotIndex(splineIndex, knotIndex));
	}

	public void KnotInserted(SplineKnotIndex index)
	{
		ShiftKnotIndices(index, 1);
	}

	public void ShiftKnotIndices(SplineKnotIndex index, int offset)
	{
		KnotLink[] knotsLink = m_KnotsLink;
		foreach (KnotLink knotLink in knotsLink)
		{
			for (int j = 0; j < knotLink.Knots.Length; j++)
			{
				SplineKnotIndex splineKnotIndex = knotLink.Knots[j];
				if (splineKnotIndex.Spline == index.Spline && splineKnotIndex.Knot >= index.Knot)
				{
					knotLink.Knots[j] = new SplineKnotIndex(splineKnotIndex.Spline, splineKnotIndex.Knot + offset);
				}
			}
		}
	}
}
