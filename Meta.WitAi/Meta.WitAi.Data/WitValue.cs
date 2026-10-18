using Meta.WitAi.Json;
using UnityEngine;

namespace Meta.WitAi.Data;

public abstract class WitValue : ScriptableObject
{
	[SerializeField]
	public string path;

	private WitResponseReference reference;

	public WitResponseReference Reference
	{
		get
		{
			if (reference == null)
			{
				reference = WitResultUtilities.GetWitResponseReference(path);
			}
			return reference;
		}
	}

	public abstract object GetValue(WitResponseNode response);

	public abstract bool Equals(WitResponseNode response, object value);

	public string ToString(WitResponseNode response)
	{
		return Reference.GetStringValue(response);
	}
}
