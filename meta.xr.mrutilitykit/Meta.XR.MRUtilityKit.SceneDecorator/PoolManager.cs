using System.Collections.Generic;
using Meta.XR.Util;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class PoolManager<K, P> where K : class where P : Pool<K>
{
	private Dictionary<K, P> pools = new Dictionary<K, P>();

	public void AddPool(K primitive, P pool)
	{
		pools.Add(primitive, pool);
	}

	public bool ContainsPool(K primitive)
	{
		return pools.ContainsKey(primitive);
	}

	public P GetPool(K primitive)
	{
		pools.TryGetValue(primitive, out var value);
		return value;
	}
}
