using Meta.WitAi.Json;
using UnityEngine.Scripting;

namespace Meta.WitAi.Data.Entities;

public abstract class WitEntityDataBase<T>
{
	public WitResponseNode responseNode;

	public string id;

	public string name;

	public string role;

	public int start;

	public int end;

	public string type;

	public string body;

	public T value;

	public float confidence;

	public bool hasData;

	public WitResponseArray entities;

	[Preserve]
	public WitEntityDataBase<T> FromEntityWitResponseNode(WitResponseNode node)
	{
		responseNode = node;
		return JsonConvert.DeserializeIntoObject(this, node);
	}

	public override string ToString()
	{
		return value.ToString();
	}
}
