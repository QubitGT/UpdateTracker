using System.Collections.Generic;
using GorillaLocomotion;
using UnityEngine;

namespace GorillaTag;

[CreateAssetMenu(fileName = "MaterialDatasSO", menuName = "GorillaTag/MaterialDatasSO")]
public class MaterialDatasSO : ScriptableObject
{
	public List<GTPlayer.MaterialData> datas;

	public List<HashWrapper> surfaceEffects;
}
