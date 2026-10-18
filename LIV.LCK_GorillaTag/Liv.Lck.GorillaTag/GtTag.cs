using System.Collections.Generic;
using UnityEngine;

namespace Liv.Lck.GorillaTag;

public class GtTag : MonoBehaviour
{
	public GtTagType gtTagType;

	private static Dictionary<GtTagType, Transform> _cache = new Dictionary<GtTagType, Transform>();

	public static bool TryGetTransform(GtTagType gtTagType, out Transform transform)
	{
		return _cache.TryGetValue(gtTagType, out transform);
	}

	private void Awake()
	{
		base.enabled = false;
	}

	private void OnEnable()
	{
		_cache.Add(gtTagType, base.transform);
	}

	private void OnDisable()
	{
		_cache.Remove(gtTagType);
	}
}
