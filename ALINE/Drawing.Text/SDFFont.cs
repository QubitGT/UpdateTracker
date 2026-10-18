using UnityEngine;

namespace Drawing.Text;

internal struct SDFFont
{
	public string name;

	public int size;

	public int width;

	public int height;

	public bool bold;

	public bool italic;

	public SDFCharacter[] characters;

	public Material material;
}
