using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace GT_CustomMapSupportRuntime;

[Preserve]
public class Descriptor
{
	[JsonProperty(PropertyName = "objectName")]
	public string objectName = "";

	[JsonConstructor]
	public Descriptor()
	{
	}
}
