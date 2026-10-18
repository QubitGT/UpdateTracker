using System;
using System.Collections.Generic;

namespace UnityEngine.AddressableAssets.ResourceLocators;

public class ContentCatalogDataEntry
{
	public string InternalId { get; set; }

	public string Provider { get; private set; }

	public List<object> Keys { get; private set; }

	public List<object> Dependencies { get; private set; }

	public object Data { get; set; }

	public Type ResourceType { get; private set; }

	public ContentCatalogDataEntry(Type type, string internalId, string provider, IEnumerable<object> keys, IEnumerable<object> dependencies = null, object extraData = null)
	{
		InternalId = internalId;
		Provider = provider;
		ResourceType = type;
		Keys = new List<object>(keys);
		Dependencies = ((dependencies == null) ? new List<object>() : new List<object>(dependencies));
		Data = extraData;
	}
}
