using UnityEngine.Serialization;

namespace UnityEngine.VFX.Utility;

[AddComponentMenu("VFX/Property Binders/Terrain Binder")]
[VFXBinder("Utility/Terrain")]
internal class VFXTerrainBinder : VFXBinderBase
{
	[VFXPropertyBinding(new string[] { "UnityEditor.VFX.TerrainType" })]
	[FormerlySerializedAs("TerrainParameter")]
	public ExposedProperty m_Property = "Terrain";

	public Terrain Terrain;

	private ExposedProperty Terrain_Bounds_center;

	private ExposedProperty Terrain_Bounds_size;

	private ExposedProperty Terrain_HeightMap;

	private ExposedProperty Terrain_Height;

	public string Property
	{
		get
		{
			return (string)m_Property;
		}
		set
		{
			m_Property = value;
			UpdateSubProperties();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		UpdateSubProperties();
	}

	private void OnValidate()
	{
		UpdateSubProperties();
	}

	private void UpdateSubProperties()
	{
		Terrain_Bounds_center = m_Property + "_Bounds_center";
		Terrain_Bounds_size = m_Property + "_Bounds_size";
		Terrain_HeightMap = m_Property + "_HeightMap";
		Terrain_Height = m_Property + "_Height";
	}

	public override bool IsValid(VisualEffect component)
	{
		if (Terrain != null && component.HasVector3(Terrain_Bounds_center) && component.HasVector3(Terrain_Bounds_size) && component.HasTexture(Terrain_HeightMap))
		{
			return component.HasFloat(Terrain_Height);
		}
		return false;
	}

	public override void UpdateBinding(VisualEffect component)
	{
		Bounds bounds = Terrain.terrainData.bounds;
		component.SetVector3(Terrain_Bounds_center, bounds.center);
		component.SetVector3(Terrain_Bounds_size, bounds.size);
		component.SetTexture(Terrain_HeightMap, Terrain.terrainData.heightmapTexture);
		component.SetFloat(Terrain_Height, Terrain.terrainData.heightmapScale.y);
	}

	public override string ToString()
	{
		return string.Format("Terrain : '{0}' -> {1}", m_Property, (Terrain == null) ? "(null)" : Terrain.name);
	}
}
