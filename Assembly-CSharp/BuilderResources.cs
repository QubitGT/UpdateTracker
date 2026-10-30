using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuilderMaterialResources", menuName = "GorillaTag/Builder/Resources", order = 0)]
public class BuilderResources : ScriptableObject
{
	public List<BuilderResourceQuantity> quantities;
}
