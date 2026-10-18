using UnityEngine;

namespace GT_CustomMapSupportRuntime;

public class CubicPlanetZoneSettings : PlanetZoneSettings
{
	[Header("box constraint for where gravity center can be")]
	[SerializeField]
	public Vector3 constraints;
}
