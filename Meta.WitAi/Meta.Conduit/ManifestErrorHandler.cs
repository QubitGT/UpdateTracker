using System.Collections.Generic;
using System.Linq;
using Meta.WitAi.Json;
using UnityEngine.Scripting;

namespace Meta.Conduit;

internal class ManifestErrorHandler : IManifestMethod
{
	[Preserve]
	public string ID { get; set; }

	[Preserve]
	public string Assembly { get; set; }

	[Preserve]
	public string Name { get; set; }

	[Preserve]
	public List<ManifestParameter> Parameters { get; set; } = new List<ManifestParameter>();


	[JsonIgnore]
	public string DeclaringTypeName => ID.Substring(0, ID.LastIndexOf('.'));

	[Preserve]
	public ManifestErrorHandler()
	{
	}

	public override bool Equals(object obj)
	{
		if (obj is ManifestAction other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (((17 * 31 + ID.GetHashCode()) * 31 + Assembly.GetHashCode()) * 31 + Name.GetHashCode()) * 31 + Parameters.GetHashCode();
	}

	private bool Equals(ManifestAction other)
	{
		if (ID == other.ID && Assembly == other.Assembly && Name == other.Name)
		{
			return Parameters.SequenceEqual(other.Parameters);
		}
		return false;
	}
}
