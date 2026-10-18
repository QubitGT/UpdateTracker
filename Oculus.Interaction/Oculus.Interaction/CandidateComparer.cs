using UnityEngine;

namespace Oculus.Interaction;

public abstract class CandidateComparer<T> : MonoBehaviour, ICandidateComparer where T : class
{
	public int Compare(object a, object b)
	{
		T val = a as T;
		T val2 = b as T;
		if (val != null && val2 != null)
		{
			return Compare(val, val2);
		}
		return 0;
	}

	public abstract int Compare(T a, T b);
}
